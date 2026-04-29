namespace Streak.Ui.UnitTests.Services;

public sealed class AutomatedBackupRunServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldRunLocalOnly_WhenOnlyLocalBackupIsEnabled()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: true, isCloudEnabled: false);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        var expectedLocation = new SavedFileLocation
        {
            SavedFileDisplayPath = "Downloads/Streak/Backups/Automated/streak-auto-data-backup-20260426-040000.zip",
            ParentFolderDisplayPath = "Downloads/Streak/Backups/Automated"
        };
        localExecutionServiceMock
            .Setup(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLocation);

        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.LocalEnabled.Should().BeTrue();
        result.LocalSucceeded.Should().BeTrue();
        result.LocalSavedLocation.Should().BeEquivalentTo(expectedLocation);
        result.CloudEnabled.Should().BeFalse();
        result.CloudSucceeded.Should().BeFalse();
        localExecutionServiceMock.Verify(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Once);
        cloudBackupServiceMock.Verify(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldRunCloudOnly_WhenOnlyCloudBackupIsEnabled()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: false, isCloudEnabled: true);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.LocalEnabled.Should().BeFalse();
        result.LocalSucceeded.Should().BeFalse();
        result.LocalSavedLocation.Should().BeNull();
        result.CloudEnabled.Should().BeTrue();
        result.CloudSucceeded.Should().BeTrue();
        localExecutionServiceMock.Verify(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Never);
        cloudBackupServiceMock.Verify(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldContinueToCloud_WhenLocalBackupFails()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: true, isCloudEnabled: true);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        localExecutionServiceMock
            .Setup(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Local save failed."));

        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.LocalEnabled.Should().BeTrue();
        result.LocalSucceeded.Should().BeFalse();
        result.LocalFailure.Should().BeOfType<InvalidOperationException>();
        result.CloudEnabled.Should().BeTrue();
        result.CloudSucceeded.Should().BeTrue();
        result.CloudFailure.Should().BeNull();
        result.CloudFailureKind.Should().BeNull();
        localExecutionServiceMock.Verify(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Once);
        cloudBackupServiceMock.Verify(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldReturnPartialSuccess_WhenCloudBackupFailsAfterLocalSuccess()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: true, isCloudEnabled: true);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        localExecutionServiceMock
            .Setup(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SavedFileLocation
            {
                SavedFileDisplayPath = "Downloads/Streak/Backups/Automated/streak-auto-data-backup-20260426-040000.zip",
                ParentFolderDisplayPath = "Downloads/Streak/Backups/Automated"
            });

        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        cloudBackupServiceMock
            .Setup(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now."));

        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.LocalSucceeded.Should().BeTrue();
        result.LocalFailure.Should().BeNull();
        result.CloudSucceeded.Should().BeFalse();
        result.CloudFailure.Should().BeOfType<OneDriveBackupException>();
        result.CloudFailureKind.Should().Be(OneDriveBackupFailureKind.NetworkUnavailable);
        result.HasAnySuccess.Should().BeTrue();
        result.HasAnyFailure.Should().BeTrue();
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldReturnFailureResult_WhenCloudOnlyBackupFails()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: false, isCloudEnabled: true);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        cloudBackupServiceMock
            .Setup(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now."));

        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.LocalEnabled.Should().BeFalse();
        result.LocalSucceeded.Should().BeFalse();
        result.LocalFailure.Should().BeNull();
        result.CloudEnabled.Should().BeTrue();
        result.CloudSucceeded.Should().BeFalse();
        result.CloudFailure.Should().BeOfType<OneDriveBackupException>();
        result.CloudFailureKind.Should().Be(OneDriveBackupFailureKind.NetworkUnavailable);
        result.HasAnySuccess.Should().BeFalse();
        result.HasAnyFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldReturnFailureResult_WhenBothDestinationsFail()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: true, isCloudEnabled: true);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        localExecutionServiceMock
            .Setup(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Local save failed."));

        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        cloudBackupServiceMock
            .Setup(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now."));

        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.LocalEnabled.Should().BeTrue();
        result.LocalSucceeded.Should().BeFalse();
        result.LocalFailure.Should().BeOfType<InvalidOperationException>();
        result.CloudEnabled.Should().BeTrue();
        result.CloudSucceeded.Should().BeFalse();
        result.CloudFailure.Should().BeOfType<OneDriveBackupException>();
        result.CloudFailureKind.Should().Be(OneDriveBackupFailureKind.NetworkUnavailable);
        result.HasAnySuccess.Should().BeFalse();
        result.HasAnyFailure.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldThrow_WhenCancellationIsRequested()
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: true, isCloudEnabled: false);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        localExecutionServiceMock
            .Setup(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cancellationTokenSource.Token));

        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var act = () => sut.ExecuteEnabledBackupsAsync(cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task ExecuteEnabledBackupsAsync_ShouldReturnEmptyResult_WhenNoDestinationsAreEnabled()
    {
        var configurationServiceMock = CreateConfigurationServiceMock(isEnabled: false, isCloudEnabled: false);
        var localExecutionServiceMock = new Mock<IAutomatedBackupExecutionService>();
        var cloudBackupServiceMock = new Mock<IAutomatedCloudBackupService>();
        var sut = CreateSut(configurationServiceMock.Object, localExecutionServiceMock.Object, cloudBackupServiceMock.Object);

        var result = await sut.ExecuteEnabledBackupsAsync();

        result.HasAnySuccess.Should().BeFalse();
        result.HasAnyFailure.Should().BeFalse();
        localExecutionServiceMock.Verify(x => x.ExecuteAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Never);
        cloudBackupServiceMock.Verify(x => x.UploadAutomatedBackupAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Private Helper Methods

    private static AutomatedBackupRunService CreateSut(
        IAutomatedBackupConfigurationService automatedBackupConfigurationService,
        IAutomatedBackupExecutionService automatedBackupExecutionService,
        IAutomatedCloudBackupService automatedCloudBackupService)
    {
        return new AutomatedBackupRunService(
            automatedBackupConfigurationService,
            automatedBackupExecutionService,
            automatedCloudBackupService,
            new Mock<ILogger<AutomatedBackupRunService>>().Object);
    }

    private static Mock<IAutomatedBackupConfigurationService> CreateConfigurationServiceMock(bool isEnabled, bool isCloudEnabled)
    {
        var configurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        configurationServiceMock.Setup(x => x.GetIsEnabled()).Returns(isEnabled);
        configurationServiceMock.Setup(x => x.GetIsCloudEnabled()).Returns(isCloudEnabled);
        configurationServiceMock.Setup(x => x.GetHasAnyEnabled()).Returns(isEnabled || isCloudEnabled);
        return configurationServiceMock;
    }

    #endregion
}
