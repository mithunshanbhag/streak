namespace Streak.Ui.UnitTests.Services;

public sealed class AutomatedBackupExecutionServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ExecuteAutomatedBackupAsync_ShouldCreateTimestampedBackupAndSaveIt()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var backupFileSaverMock = new Mock<IAutomatedBackupFileSaver>();
        string? savedBackupPath = null;
        string? inspectedBackupPath = null;

        backupFileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns<string, CancellationToken>((filePath, _) =>
            {
                savedBackupPath = filePath;
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-auto-backup.db");
                File.Copy(filePath, inspectedBackupPath, true);

                return Task.FromResult(new SavedFileLocation
                {
                    SavedFileDisplayPath = $"{StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath}/{Path.GetFileName(filePath)}",
                    ParentFolderDisplayPath = StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath
                });
            });

        var sut = CreateSut(
            sourceDatabasePath,
            exportDirectory.Path,
            backupFileSaverMock.Object);

        var savedLocation = await sut.ExecuteAutomatedBackupAsync();

        savedLocation.SavedFileDisplayPath.Should().MatchRegex("^Downloads/Streak/Backups/Automated/streak-auto-backup-[0-9]{8}-[0-9]{6}\\.db$");
        savedLocation.ParentFolderDisplayPath.Should().Be(StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath);
        savedBackupPath.Should().NotBeNull();
        inspectedBackupPath.Should().NotBeNull();
        Path.GetFileName(savedBackupPath!).Should().MatchRegex("^streak-auto-backup-[0-9]{8}-[0-9]{6}\\.db$");
        File.Exists(savedBackupPath!).Should().BeFalse();
        File.Exists(inspectedBackupPath!).Should().BeTrue();

        using var connection = OpenConnection(inspectedBackupPath!);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Sample;";

        var recordCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        recordCount.Should().Be(1);

        backupFileSaverMock.Verify(
            x => x.SaveBackupAsync(savedBackupPath!, It.IsAny<CancellationToken>()),
            Times.Once);
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
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var backupFileSaverMock = new Mock<IAutomatedBackupFileSaver>();
        backupFileSaverMock
            .Setup(x => x.SaveBackupAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed."));

        var sut = CreateSut(
            sourceDatabasePath,
            exportDirectory.Path,
            backupFileSaverMock.Object);

        var act = () => sut.ExecuteAutomatedBackupAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Save failed.");

        Directory.GetFiles(exportDirectory.Path, "streak-auto-backup-*.db").Should().BeEmpty();
    }

    #endregion

    #region Private Helper Methods

    private static AutomatedBackupExecutionService CreateSut(
        string sourceDatabasePath,
        string exportDirectoryPath,
        IAutomatedBackupFileSaver backupFileSaver)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(sourceDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);

        var loggerMock = new Mock<ILogger<AutomatedBackupExecutionService>>();

        return new AutomatedBackupExecutionService(
            appStoragePathServiceMock.Object,
            backupFileSaver,
            loggerMock.Object);
    }

    private static void SeedDatabase(string sourceDatabasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(sourceDatabasePath)!);

        using var connection = OpenConnection(sourceDatabasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE Sample (
                Id INTEGER PRIMARY KEY
            );

            INSERT INTO Sample (Id) VALUES (1);
            """;
        command.ExecuteNonQuery();
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
