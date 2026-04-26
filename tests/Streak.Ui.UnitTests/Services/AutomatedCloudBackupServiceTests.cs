namespace Streak.Ui.UnitTests.Services;

public sealed class AutomatedCloudBackupServiceTests
{
    #region Positive tests

    [Fact]
    public async Task UploadAutomatedBackupAsync_ShouldUploadArchive()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var workingArchivePath = Path.Combine(temporaryDirectory.Path, "streak-auto-data-backup-20260426-040000.zip");
        await File.WriteAllTextAsync(workingArchivePath, "backup");

        var backupArchiveFactoryMock = new Mock<IBackupArchiveFactory>();
        backupArchiveFactoryMock
            .Setup(x => x.CreateAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupArchiveArtifact
            {
                WorkingFilePath = workingArchivePath,
                UnavailableReferencedProofPaths = []
            });

        var oneDriveBackupUploadClientMock = new Mock<IOneDriveBackupUploadClient>();

        var sut = new AutomatedCloudBackupService(
            backupArchiveFactoryMock.Object,
            oneDriveBackupUploadClientMock.Object,
            new Mock<ILogger<AutomatedCloudBackupService>>().Object);

        await sut.UploadAutomatedBackupAsync();

        oneDriveBackupUploadClientMock.Verify(
            x => x.UploadAutomatedBackupAsync(
                workingArchivePath,
                Path.GetFileName(workingArchivePath),
                It.IsAny<CancellationToken>()),
            Times.Once);
        File.Exists(workingArchivePath).Should().BeFalse();
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task UploadAutomatedBackupAsync_ShouldDeleteTemporaryArchive_WhenUploadFails()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var workingArchivePath = Path.Combine(temporaryDirectory.Path, "streak-auto-data-backup-20260426-040100.zip");
        await File.WriteAllTextAsync(workingArchivePath, "backup");

        var backupArchiveFactoryMock = new Mock<IBackupArchiveFactory>();
        backupArchiveFactoryMock
            .Setup(x => x.CreateAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupArchiveArtifact
            {
                WorkingFilePath = workingArchivePath,
                UnavailableReferencedProofPaths = []
            });

        var oneDriveBackupUploadClientMock = new Mock<IOneDriveBackupUploadClient>();
        oneDriveBackupUploadClientMock
            .Setup(x => x.UploadAutomatedBackupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now."));

        var sut = new AutomatedCloudBackupService(
            backupArchiveFactoryMock.Object,
            oneDriveBackupUploadClientMock.Object,
            new Mock<ILogger<AutomatedCloudBackupService>>().Object);

        var act = () => sut.UploadAutomatedBackupAsync();

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.NetworkUnavailable);
        File.Exists(workingArchivePath).Should().BeFalse();
    }

    #endregion

    #region Private Helper Methods

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
