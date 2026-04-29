namespace Streak.Ui.UnitTests.Services;

public sealed class AutomatedBackupExecutionServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ExecuteAutomatedBackupAsync_ShouldCreateTimestampedBackupAndSaveIt()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        var proofRelativePath = "Habit-1/2026/04/2026-04-21/proof.jpg";
        var expectedProofBytes = new byte[] { 4, 3, 2, 1 };

        CreateProofFile(proofDirectory.Path, proofRelativePath, expectedProofBytes);
        SeedDatabase(sourceDatabasePath, proofRelativePath);

        var backupFileSaverMock = new Mock<IAutomatedBackupFileSaver>();
        string? savedBackupPath = null;
        string? inspectedBackupPath = null;

        backupFileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((filePath, _) =>
            {
                savedBackupPath = filePath;
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-auto-backup.zip");
                File.Copy(filePath, inspectedBackupPath, true);

                return Task.FromResult(new SavedFileLocation
                {
                    SavedFileDisplayPath = $"{StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath}/{Path.GetFileName(filePath)}",
                    ParentFolderDisplayPath = StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath
                });
            });

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            backupFileSaverMock.Object);

        var savedLocation = await sut.ExecuteAutomatedBackupAsync();

        savedLocation.SavedFileDisplayPath.Should().MatchRegex("^Downloads/Streak/Backups/Automated/streak-auto-data-backup-[0-9]{8}-[0-9]{6}\\.zip$");
        savedLocation.ParentFolderDisplayPath.Should().Be(StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath);
        savedBackupPath.Should().NotBeNull();
        inspectedBackupPath.Should().NotBeNull();
        Path.GetFileName(savedBackupPath!).Should().MatchRegex("^streak-auto-data-backup-[0-9]{8}-[0-9]{6}\\.zip$");
        File.Exists(savedBackupPath!).Should().BeFalse();
        File.Exists(inspectedBackupPath!).Should().BeTrue();

        using var archive = ZipFile.OpenRead(inspectedBackupPath!);
        archive.Entries.Select(entry => entry.FullName).Should().Contain([
            "streak.db",
            $"CheckinProofs/{proofRelativePath}"
        ]);

        var extractedDatabasePath = Path.Combine(exportDirectory.Path, "inspected-auto-backup.db");
        ExtractArchiveEntryToFile(archive, "streak.db", extractedDatabasePath);

        using var connection = OpenConnection(extractedDatabasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Sample;";

        var recordCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        recordCount.Should().Be(1);

        ReadArchiveEntryBytes(archive, $"CheckinProofs/{proofRelativePath}")
            .Should()
            .Equal(expectedProofBytes);

        backupFileSaverMock.Verify(
            x => x.SaveBackupAsync(savedBackupPath!, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAutomatedBackupAsync_ShouldSkipUnavailableProofFilesAndStillSaveBackup()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        var missingProofRelativePath = "Habit-1/2026/04/2026-04-21/missing-proof.jpg";

        SeedDatabase(sourceDatabasePath, missingProofRelativePath);

        var backupFileSaverMock = new Mock<IAutomatedBackupFileSaver>();
        string? inspectedBackupPath = null;

        backupFileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((filePath, _) =>
            {
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-missing-proof-auto-backup.zip");
                File.Copy(filePath, inspectedBackupPath, true);

                return Task.FromResult(new SavedFileLocation
                {
                    SavedFileDisplayPath = $"{StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath}/{Path.GetFileName(filePath)}",
                    ParentFolderDisplayPath = StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath
                });
            });

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            backupFileSaverMock.Object);

        var savedLocation = await sut.ExecuteAutomatedBackupAsync();

        savedLocation.ParentFolderDisplayPath.Should().Be(StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath);
        inspectedBackupPath.Should().NotBeNull();

        using var archive = ZipFile.OpenRead(inspectedBackupPath!);
        var entryNames = archive.Entries.Select(entry => entry.FullName).ToList();
        entryNames.Should().Contain("streak.db");
        entryNames.Should().NotContain($"CheckinProofs/{missingProofRelativePath}");
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task ExecuteAutomatedBackupAsync_ShouldThrow_WhenSourceDatabaseDoesNotExist()
    {
        using var exportDirectory = new TemporaryDirectory();

        var backupFileSaverMock = new Mock<IAutomatedBackupFileSaver>();
        var sut = CreateSut(
            Path.Combine(exportDirectory.Path, "missing.db"),
            exportDirectory.Path,
            exportDirectory.Path,
            backupFileSaverMock.Object);

        var act = () => sut.ExecuteAutomatedBackupAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();

        backupFileSaverMock.Verify(
            x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task ExecuteAutomatedBackupAsync_ShouldDeleteTemporaryBackup_WhenSaverFails()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var backupFileSaverMock = new Mock<IAutomatedBackupFileSaver>();
        backupFileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed."));

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            backupFileSaverMock.Object);

        var act = () => sut.ExecuteAutomatedBackupAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Save failed.");

        Directory.GetFiles(exportDirectory.Path, "streak-auto-data-backup-*.zip").Should().BeEmpty();
    }

    #endregion

    #region Private Helper Methods

    private static AutomatedBackupExecutionService CreateSut(
        string sourceDatabasePath,
        string checkinProofsDirectoryPath,
        string exportDirectoryPath,
        IAutomatedBackupFileSaver backupFileSaver)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(sourceDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(checkinProofsDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);

        var loggerMock = new Mock<ILogger<AutomatedBackupExecutionService>>();
        var backupArchiveFactory = new BackupArchiveFactory(
            appStoragePathServiceMock.Object,
            new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object),
            TimeProvider.System);

        return new AutomatedBackupExecutionService(
            backupArchiveFactory,
            backupFileSaver,
            loggerMock.Object);
    }

    private static void SeedDatabase(string sourceDatabasePath, string? proofImageUri = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(sourceDatabasePath)!);

        using var connection = OpenConnection(sourceDatabasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE Sample (
                Id INTEGER PRIMARY KEY
            );

            CREATE TABLE Checkins (
                HabitId INTEGER NOT NULL,
                CheckinDate TEXT NOT NULL,
                ProofImageUri TEXT NULL,
                ProofImageDisplayName TEXT NULL,
                ProofImageSizeBytes INTEGER NULL,
                ProofImageModifiedOn TEXT NULL,
                PRIMARY KEY (HabitId, CheckinDate)
            );

            INSERT INTO Sample (Id) VALUES (1);

            INSERT INTO Checkins (HabitId, CheckinDate, ProofImageUri, ProofImageDisplayName, ProofImageSizeBytes, ProofImageModifiedOn)
            VALUES (1, '2026-04-21', $proofImageUri, 'proof.jpg', 4, '2026-04-21T08:30:12.0000000+00:00');
            """;
        command.Parameters.AddWithValue("$proofImageUri", (object?)proofImageUri ?? DBNull.Value);
        command.ExecuteNonQuery();
    }

    private static void CreateProofFile(string proofDirectoryPath, string relativeProofPath, byte[] proofBytes)
    {
        var proofPath = Path.Combine(
            [.. new[] { proofDirectoryPath }, .. relativeProofPath.Split('/', StringSplitOptions.RemoveEmptyEntries)]);
        Directory.CreateDirectory(Path.GetDirectoryName(proofPath)!);
        File.WriteAllBytes(proofPath, proofBytes);
    }

    private static void ExtractArchiveEntryToFile(ZipArchive archive, string entryPath, string destinationPath)
    {
        var entry = archive.GetEntry(entryPath);
        entry.Should().NotBeNull();

        using var sourceStream = entry!.Open();
        using var destinationStream = File.Create(destinationPath);
        sourceStream.CopyTo(destinationStream);
    }

    private static byte[] ReadArchiveEntryBytes(ZipArchive archive, string entryPath)
    {
        var entry = archive.GetEntry(entryPath);
        entry.Should().NotBeNull();

        using var sourceStream = entry!.Open();
        using var memoryStream = new MemoryStream();
        sourceStream.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }

    private static SqliteConnection OpenConnection(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"streak-ui-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
    }

    #endregion
}
