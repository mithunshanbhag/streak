namespace Streak.Ui.UnitTests.Services;

public sealed class DatabaseShareServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ShareDatabaseAsync_ShouldCreateTimestampedBackupAndPassItToShareRequest()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        ShareFileRequest? shareRequest = null;
        string? inspectedBackupPath = null;

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns<ShareFileRequest>(request =>
            {
                shareRequest = request;
                inspectedBackupPath = Path.Combine(exportDirectory.Path, "inspected-backup.db");
                File.Copy(request.File.FullPath, inspectedBackupPath, true);

                return Task.CompletedTask;
            });

        var sut = CreateSut(
            sourceDatabasePath,
            exportDirectory.Path,
            shareMock.Object);

        await sut.ShareDatabaseAsync();

        sut.CanShare.Should().BeTrue();
        shareRequest.Should().NotBeNull();
        inspectedBackupPath.Should().NotBeNull();

        var actualShareRequest = shareRequest!;
        var actualInspectedBackupPath = inspectedBackupPath!;

        actualShareRequest.Title.Should().Be("Share DB");
        actualShareRequest.File.FullPath.Should().MatchRegex("^.+streak-backup-[0-9]{8}-[0-9]{6}\\.db$");
        File.Exists(actualShareRequest.File.FullPath).Should().BeTrue();
        File.Exists(actualInspectedBackupPath).Should().BeTrue();

        using var connection = OpenConnection(actualInspectedBackupPath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM Sample;";

        var recordCount = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        recordCount.Should().Be(1);
    }

    [Fact]
    public async Task ShareDatabaseAsync_ShouldDeleteOlderCachedBackupsBeforeCreatingANewOne()
    {
        using var sourceDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var staleBackupPath = Path.Combine(exportDirectory.Path, "streak-backup-20000101-010101.db");
        File.WriteAllText(staleBackupPath, "stale");

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(
            sourceDatabasePath,
            exportDirectory.Path,
            shareMock.Object);

        await sut.ShareDatabaseAsync();

        File.Exists(staleBackupPath).Should().BeFalse();
        Directory.GetFiles(exportDirectory.Path, "streak-backup-*.db").Should().HaveCount(1);
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
        using var exportDirectory = new TemporaryDirectory();

        var sourceDatabasePath = Path.Combine(sourceDirectory.Path, "streak.local.db");
        SeedDatabase(sourceDatabasePath);

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .ThrowsAsync(new InvalidOperationException("Share failed."));

        var sut = CreateSut(
            sourceDatabasePath,
            exportDirectory.Path,
            shareMock.Object);

        var act = () => sut.ShareDatabaseAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Share failed.");

        Directory.GetFiles(exportDirectory.Path, "streak-backup-*.db").Should().HaveCount(1);
    }

    #endregion

    #region Private Helper Methods

    private static DatabaseShareService CreateSut(
        string sourceDatabasePath,
        string exportDirectoryPath,
        IShare share)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(sourceDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);

        var loggerMock = new Mock<ILogger<DatabaseShareService>>();

        return new DatabaseShareService(
            appStoragePathServiceMock.Object,
            share,
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
