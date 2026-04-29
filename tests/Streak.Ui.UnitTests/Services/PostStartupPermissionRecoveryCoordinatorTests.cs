namespace Streak.Ui.UnitTests.Services;

public sealed class PostStartupPermissionRecoveryCoordinatorTests
{
    #region Positive tests

    [Fact]
    public async Task RecoverMissingPermissionsAfterHomepageRenderAsync_ShouldRequestNotificationAndCameraPermissions_WhenRelevantFeaturesAreEnabled()
    {
        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        reminderConfigurationServiceMock
            .Setup(x => x.GetIsEnabled())
            .Returns(true);

        var automatedBackupConfigurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        automatedBackupConfigurationServiceMock
            .Setup(x => x.GetHasAnyEnabled())
            .Returns(true);

        var reminderNotificationPermissionServiceMock = new Mock<IReminderNotificationPermissionService>();
        reminderNotificationPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock
            .SetupGet(x => x.SupportsCameraCapture)
            .Returns(true);

        var cameraPermissionServiceMock = new Mock<ICameraPermissionService>();
        cameraPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new PostStartupPermissionRecoveryCoordinator(
            reminderConfigurationServiceMock.Object,
            automatedBackupConfigurationServiceMock.Object,
            reminderNotificationPermissionServiceMock.Object,
            checkinProofServiceMock.Object,
            cameraPermissionServiceMock.Object,
            Mock.Of<ILogger<PostStartupPermissionRecoveryCoordinator>>());

        await sut.RecoverMissingPermissionsAfterHomepageRenderAsync();

        reminderNotificationPermissionServiceMock.Verify(
            x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        cameraPermissionServiceMock.Verify(
            x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task RecoverMissingPermissionsAfterHomepageRenderAsync_ShouldOnlyRunOncePerAppSession()
    {
        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        reminderConfigurationServiceMock
            .Setup(x => x.GetIsEnabled())
            .Returns(false);

        var automatedBackupConfigurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        automatedBackupConfigurationServiceMock
            .Setup(x => x.GetHasAnyEnabled())
            .Returns(false);

        var reminderNotificationPermissionServiceMock = new Mock<IReminderNotificationPermissionService>();
        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock
            .SetupGet(x => x.SupportsCameraCapture)
            .Returns(true);
        var cameraPermissionServiceMock = new Mock<ICameraPermissionService>();
        cameraPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new PostStartupPermissionRecoveryCoordinator(
            reminderConfigurationServiceMock.Object,
            automatedBackupConfigurationServiceMock.Object,
            reminderNotificationPermissionServiceMock.Object,
            checkinProofServiceMock.Object,
            cameraPermissionServiceMock.Object,
            Mock.Of<ILogger<PostStartupPermissionRecoveryCoordinator>>());

        await sut.RecoverMissingPermissionsAfterHomepageRenderAsync();
        await sut.RecoverMissingPermissionsAfterHomepageRenderAsync();

        reminderNotificationPermissionServiceMock.Verify(
            x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        cameraPermissionServiceMock.Verify(
            x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
