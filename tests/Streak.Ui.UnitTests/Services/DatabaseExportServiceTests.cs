namespace Streak.Ui.UnitTests.Services;

public sealed class DatabaseExportServiceTests
{
    [Fact]
    public async Task ExportDatabaseAsync_ShouldCreateTimestampedBackupAndSaveIt()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        var proofRelativePath = "Habit-1/2026/04/2026-04-21/proof.jpg";
        var expectedProofBytes = new byte[] { 1, 2, 3, 4 };

        CreateProofFile(proofDirectory.Path, proofRelativePath, expectedProofBytes);
        SeedDatabase(sourceDatabasePath, proofRelativePath);

        var fileSaverMock = new Mock<IDatabaseExportFileSaver>();
        string? savedBackupPath = null;
        string? inspectedBackupPath = null;

        fileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((filePath, _) =>
            {
                savedBackupPath = filePath;
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-backup.zip");
                File.Copy(filePath, inspectedBackupPath, true);

                return Task.FromResult(DatabaseExportResult.Saved(new SavedFileLocation
                {
                    SavedFileDisplayPath = $"{StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath}/{Path.GetFileName(filePath)}",
                    ParentFolderDisplayPath = StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath
                }));
            });

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            fileSaverMock.Object);

        var exportResult = await sut.ExportDatabaseAsync();

        exportResult.Status.Should().Be(DatabaseExportStatus.Saved);
        exportResult.SavedFileLocation.Should().NotBeNull();
        exportResult.SavedFileLocation!.ParentFolderDisplayPath.Should().Be(StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath);
        savedBackupPath.Should().NotBeNull();
        Path.GetFileName(savedBackupPath!).Should().MatchRegex("^streak-data-backup-[0-9]{8}-[0-9]{6}\\.zip$");
        File.Exists(savedBackupPath!).Should().BeFalse();
        File.Exists(inspectedBackupPath!).Should().BeTrue();

        using var archive = ZipFile.OpenRead(inspectedBackupPath!);
        archive.Entries.Select(entry => entry.FullName).Should().Contain([
            "streak.db",
            $"CheckinProofs/{proofRelativePath}"
        ]);

        var extractedDatabasePath = Path.Combine(exportDirectory.Path, "inspected-backup.db");
        ExtractArchiveEntryToFile(archive, "streak.db", extractedDatabasePath);

        var backupConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = extractedDatabasePath,
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(backupConnectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Sample;";

        var recordCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        recordCount.Should().Be(1);

        ReadArchiveEntryBytes(archive, $"CheckinProofs/{proofRelativePath}")
            .Should()
            .Equal(expectedProofBytes);

        fileSaverMock.Verify(
            x => x.SaveBackupAsync(savedBackupPath!, It.IsAny<CancellationToken>()),
            Times.Once);

        Directory.GetFiles(exportDirectory.Path, "streak-data-backup-*.zip").Should().BeEmpty();
    }

    [Fact]
    public async Task ExportDatabaseAsync_ShouldThrow_WhenSourceDatabaseDoesNotExist()
    {
        using var exportDirectory = new TemporaryDirectory();

        var fileSaverMock = new Mock<IDatabaseExportFileSaver>();

        var sut = CreateSut(
            Path.Combine(exportDirectory.Path, "missing.db"),
            exportDirectory.Path,
            exportDirectory.Path,
            fileSaverMock.Object);

        var act = () => sut.ExportDatabaseAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();

        fileSaverMock.Verify(
            x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExportDatabaseAsync_ShouldReturnCancelled_WhenSaveIsCancelled()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var fileSaverMock = new Mock<IDatabaseExportFileSaver>();
        fileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(DatabaseExportResult.Cancelled);

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            fileSaverMock.Object);

        var exportResult = await sut.ExportDatabaseAsync();

        exportResult.Status.Should().Be(DatabaseExportStatus.Cancelled);
        Directory.GetFiles(exportDirectory.Path).Should().BeEmpty();
    }

    [Fact]
    public async Task ExportDatabaseAsync_ShouldPropagateSaveFailures()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var fileSaverMock = new Mock<IDatabaseExportFileSaver>();
        fileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Share failed."));

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            fileSaverMock.Object);

        var act = () => sut.ExportDatabaseAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Share failed.");

        Directory.GetFiles(exportDirectory.Path).Should().BeEmpty();
    }

    private static DatabaseExportService CreateSut(
        string sourceDatabasePath,
        string checkinProofsDirectoryPath,
        string exportDirectoryPath,
        IDatabaseExportFileSaver fileSaver)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(sourceDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(checkinProofsDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);

        var loggerMock = new Mock<ILogger<DatabaseExportService>>();

        return new DatabaseExportService(
            appStoragePathServiceMock.Object,
            fileSaver,
            loggerMock.Object);
    }

    private static void SeedDatabase(string sourceDatabasePath, string? proofImageUri = null)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(sourceDatabasePath)!);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = sourceDatabasePath,
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

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
}
