namespace Streak.Ui.UnitTests.Services;

public sealed class ReminderNotificationPermissionCoordinatorTests
{
    #region Positive tests

    [Fact]
    public async Task RequestPermissionIfRemindersEnabledAsync_ShouldRequestPermission_WhenRemindersAreEnabled()
    {
        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        var reminderNotificationPermissionServiceMock = new Mock<IReminderNotificationPermissionService>();
        reminderConfigurationServiceMock
            .Setup(x => x.GetIsEnabled())
            .Returns(true);
        reminderNotificationPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var sut = new ReminderNotificationPermissionCoordinator(
            reminderConfigurationServiceMock.Object,
            reminderNotificationPermissionServiceMock.Object);

        var result = await sut.RequestPermissionIfRemindersEnabledAsync();

        result.Should().BeTrue();
        reminderNotificationPermissionServiceMock.Verify(
            x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task RequestPermissionIfRemindersEnabledAsync_ShouldSkipPermissionRequest_WhenRemindersAreDisabled()
    {
        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        var reminderNotificationPermissionServiceMock = new Mock<IReminderNotificationPermissionService>();
        reminderConfigurationServiceMock
            .Setup(x => x.GetIsEnabled())
            .Returns(false);
        var sut = new ReminderNotificationPermissionCoordinator(
            reminderConfigurationServiceMock.Object,
            reminderNotificationPermissionServiceMock.Object);

        var result = await sut.RequestPermissionIfRemindersEnabledAsync();

        result.Should().BeTrue();
        reminderNotificationPermissionServiceMock.Verify(
            x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
