namespace Streak.Ui.UnitTests.Services;

public sealed class ManualCloudBackupServiceTests
{
    #region Positive tests

    [Fact]
    public async Task UploadManualBackupAsync_ShouldUploadArchiveAndPersistCloudTimestamp()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var workingArchivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040000.zip");
        await File.WriteAllTextAsync(workingArchivePath, "backup");

        var backupArchiveFactoryMock = new Mock<IBackupArchiveFactory>();
        backupArchiveFactoryMock
            .Setup(x => x.CreateManualBackupAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupArchiveArtifact
            {
                WorkingFilePath = workingArchivePath,
                UnavailableReferencedProofPaths = []
            });

        var oneDriveBackupUploadClientMock = new Mock<IOneDriveBackupUploadClient>();
        var manualBackupStatusStoreMock = new Mock<IManualBackupStatusStore>();
        var fixedNow = new DateTimeOffset(2026, 04, 26, 05, 30, 0, TimeSpan.Zero);

        var sut = new ManualCloudBackupService(
            backupArchiveFactoryMock.Object,
            oneDriveBackupUploadClientMock.Object,
            manualBackupStatusStoreMock.Object,
            new FixedTimeProvider(fixedNow),
            new Mock<ILogger<ManualCloudBackupService>>().Object);

        await sut.UploadManualBackupAsync();

        oneDriveBackupUploadClientMock.Verify(
            x => x.UploadManualBackupAsync(
                workingArchivePath,
                Path.GetFileName(workingArchivePath),
                It.IsAny<CancellationToken>()),
            Times.Once);
        manualBackupStatusStoreMock.Verify(
            x => x.SetLastSuccessfulBackupUtc(ManualBackupLocation.Cloud, fixedNow),
            Times.Once);
        File.Exists(workingArchivePath).Should().BeFalse();
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task UploadManualBackupAsync_ShouldDeleteTemporaryArchive_WhenUploadFails()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var workingArchivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040100.zip");
        await File.WriteAllTextAsync(workingArchivePath, "backup");

        var backupArchiveFactoryMock = new Mock<IBackupArchiveFactory>();
        backupArchiveFactoryMock
            .Setup(x => x.CreateManualBackupAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BackupArchiveArtifact
            {
                WorkingFilePath = workingArchivePath,
                UnavailableReferencedProofPaths = []
            });

        var oneDriveBackupUploadClientMock = new Mock<IOneDriveBackupUploadClient>();
        oneDriveBackupUploadClientMock
            .Setup(x => x.UploadManualBackupAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now."));

        var manualBackupStatusStoreMock = new Mock<IManualBackupStatusStore>();

        var sut = new ManualCloudBackupService(
            backupArchiveFactoryMock.Object,
            oneDriveBackupUploadClientMock.Object,
            manualBackupStatusStoreMock.Object,
            new FixedTimeProvider(new DateTimeOffset(2026, 04, 26, 05, 30, 0, TimeSpan.Zero)),
            new Mock<ILogger<ManualCloudBackupService>>().Object);

        var act = () => sut.UploadManualBackupAsync();

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.NetworkUnavailable);

        manualBackupStatusStoreMock.Verify(
            x => x.SetLastSuccessfulBackupUtc(It.IsAny<ManualBackupLocation>(), It.IsAny<DateTimeOffset>()),
            Times.Never);
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

    private sealed class FixedTimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }

    #endregion
}
