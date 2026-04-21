namespace Streak.Ui.UnitTests.Services;

public sealed class DatabaseShareServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ShareDatabaseAsync_ShouldCreateTimestampedBackupAndPassItToShareRequest()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        var proofRelativePath = "Habit-1/2026/04/2026-04-21/proof.jpg";
        var expectedProofBytes = new byte[] { 5, 6, 7, 8 };

        CreateProofFile(proofDirectory.Path, proofRelativePath, expectedProofBytes);
        SeedDatabase(sourceDatabasePath, proofRelativePath);

        ShareFileRequest? shareRequest = null;
        string? inspectedBackupPath = null;

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns<ShareFileRequest>(request =>
            {
                shareRequest = request;
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-backup.zip");
                File.Copy(request.File.FullPath, inspectedBackupPath, true);

                return Task.CompletedTask;
            });

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            shareMock.Object);

        await sut.ShareDatabaseAsync();

        sut.CanShare.Should().BeTrue();
        shareRequest.Should().NotBeNull();
        inspectedBackupPath.Should().NotBeNull();

        var actualShareRequest = shareRequest!;
        var actualInspectedBackupPath = inspectedBackupPath!;

        actualShareRequest.Title.Should().Be("Share data");
        actualShareRequest.File.FullPath.Should().MatchRegex("^.+streak-data-backup-[0-9]{8}-[0-9]{6}\\.zip$");
        File.Exists(actualShareRequest.File.FullPath).Should().BeTrue();
        File.Exists(actualInspectedBackupPath).Should().BeTrue();

        using var archive = ZipFile.OpenRead(actualInspectedBackupPath);
        archive.Entries.Select(entry => entry.FullName).Should().Contain([
            "streak.db",
            $"CheckinProofs/{proofRelativePath}"
        ]);

        var extractedDatabasePath = Path.Combine(exportDirectory.Path, "inspected-share-backup.db");
        ExtractArchiveEntryToFile(archive, "streak.db", extractedDatabasePath);

        using var connection = OpenConnection(extractedDatabasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Sample;";

        var recordCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        recordCount.Should().Be(1);

        ReadArchiveEntryBytes(archive, $"CheckinProofs/{proofRelativePath}")
            .Should()
            .Equal(expectedProofBytes);
    }

    [Fact]
    public async Task ShareDatabaseAsync_ShouldDeleteOlderCachedBackupsBeforeCreatingANewOne()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var staleBackupPath = Path.Combine(exportDirectory.Path, "streak-data-backup-20000101-010101.zip");
        File.WriteAllText(staleBackupPath, "stale");

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            shareMock.Object);

        await sut.ShareDatabaseAsync();

        File.Exists(staleBackupPath).Should().BeFalse();
        Directory.GetFiles(exportDirectory.Path, "streak-data-backup-*.zip").Should().HaveCount(1);
    }

    [Fact]
    public async Task ShareDatabaseAsync_ShouldSkipUnavailableProofFilesAndStillOpenShareSheet()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        var missingProofRelativePath = "Habit-1/2026/04/2026-04-21/missing-proof.jpg";

        SeedDatabase(sourceDatabasePath, missingProofRelativePath);

        ShareFileRequest? shareRequest = null;
        string? inspectedBackupPath = null;

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns<ShareFileRequest>(request =>
            {
                shareRequest = request;
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-missing-proof-share.zip");
                File.Copy(request.File.FullPath, inspectedBackupPath, true);

                return Task.CompletedTask;
            });

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            shareMock.Object);

        await sut.ShareDatabaseAsync();

        shareRequest.Should().NotBeNull();
        inspectedBackupPath.Should().NotBeNull();

        using var archive = ZipFile.OpenRead(inspectedBackupPath!);
        var entryNames = archive.Entries.Select(entry => entry.FullName).ToList();
        entryNames.Should().Contain("streak.db");
        entryNames.Should().NotContain($"CheckinProofs/{missingProofRelativePath}");
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task ShareDatabaseAsync_ShouldThrow_WhenSourceDatabaseDoesNotExist()
    {
        using var exportDirectory = new TemporaryDirectory();

        var shareMock = new Mock<IShare>();

        var sut = CreateSut(
            Path.Combine(exportDirectory.Path, "missing.db"),
            exportDirectory.Path,
            exportDirectory.Path,
            shareMock.Object);

        var act = () => sut.ShareDatabaseAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();

        shareMock.Verify(
            x => x.RequestAsync(It.IsAny<ShareFileRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task ShareDatabaseAsync_ShouldPropagateShareFailuresAndKeepBackupForRetry()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var proofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .ThrowsAsync(new InvalidOperationException("Share failed."));

        var sut = CreateSut(
            sourceDatabasePath,
            proofDirectory.Path,
            exportDirectory.Path,
            shareMock.Object);

        var act = () => sut.ShareDatabaseAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Share failed.");

        Directory.GetFiles(exportDirectory.Path, "streak-data-backup-*.zip").Should().HaveCount(1);
    }

    #endregion

    #region Private Helper Methods

    private static DatabaseShareService CreateSut(
        string sourceDatabasePath,
        string checkinProofsDirectoryPath,
        string exportDirectoryPath,
        IShare share)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(sourceDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(checkinProofsDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);

        var loggerMock = new Mock<ILogger<DatabaseShareService>>();

        return new DatabaseShareService(
            appStoragePathServiceMock.Object,
            new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object),
            share,
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
