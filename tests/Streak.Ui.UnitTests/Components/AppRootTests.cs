namespace Streak.Ui.UnitTests.Components;

using Microsoft.AspNetCore.Components;
using Streak.Ui.Models.ViewModels;
using Streak.Ui.Services.Interfaces;
using Streak.Ui.Services.Models;
using Streak.Ui.Components;

public sealed class AppRootTests : TestContext
{
    public AppRootTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        Services.AddLogging();
        Services.AddSingleton(Mock.Of<ILogger<AppRoot>>());
        Services.AddSingleton(CreateOneDriveAuthReturnRouteStoreMock().Object);
        Services.AddSingleton(TimeProvider.System);
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void AppRoot_ShouldDelayRouteRendering_UntilStartupInitializationCompletes()
    {
        var initializationTcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var initializationServiceMock = new Mock<IAppInitializationService>();
        initializationServiceMock
            .Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>()))
            .Returns(initializationTcs.Task);

        var checkinServiceMock = new Mock<ICheckinService>();
        checkinServiceMock
            .Setup(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HabitCheckinViewModel>());

        RegisterHomeServices(checkinServiceMock);
        Services.AddSingleton(initializationServiceMock.Object);

        var cut = RenderComponent<AppRoot>();

        cut.Markup.Should().Contain("Preparing Streak...");
        checkinServiceMock.Verify(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()), Times.Never);

        initializationTcs.SetResult();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain("Preparing Streak...");
            cut.Markup.Should().Contain("New Habit");
            checkinServiceMock.Verify(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()), Times.Once);
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public void AppRoot_ShouldShowStartupError_WhenInitializationFails()
    {
        var initializationServiceMock = new Mock<IAppInitializationService>();
        initializationServiceMock
            .Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromException(new InvalidOperationException("boom")));

        RegisterHomeServices(new Mock<ICheckinService>());
        Services.AddSingleton(initializationServiceMock.Object);

        var cut = RenderComponent<AppRoot>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Streak couldn't finish starting.");
            cut.Markup.Should().Contain("Close and reopen the app.");
        });
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void AppRoot_ShouldNavigateToSettings_WhenOneDriveAuthReturnRouteIsPending()
    {
        var oneDriveAuthReturnRouteStoreMock = CreateOneDriveAuthReturnRouteStoreMock(RouteConstants.Settings);
        Services.AddSingleton(oneDriveAuthReturnRouteStoreMock.Object);

        var initializationServiceMock = new Mock<IAppInitializationService>();
        initializationServiceMock
            .Setup(x => x.EnsureInitializedAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        RegisterHomeServices(new Mock<ICheckinService>());
        RegisterSettingsServices();
        Services.AddSingleton(initializationServiceMock.Object);

        var cut = RenderComponent<AppRoot>();

        cut.WaitForAssertion(() =>
        {
            Services.GetRequiredService<NavigationManager>().Uri.Should().EndWith(RouteConstants.Settings);
            cut.Markup.Should().Contain("OneDrive backup");
        });
    }

    #endregion

    #region Private Helper Methods

    private static Mock<IOneDriveAuthReturnRouteStore> CreateOneDriveAuthReturnRouteStoreMock(string? pendingRoute = null)
    {
        var storeMock = new Mock<IOneDriveAuthReturnRouteStore>();
        storeMock
            .Setup(x => x.ConsumePendingReturnRoute())
            .Returns(pendingRoute);
        return storeMock;
    }

    private void RegisterHomeServices(Mock<ICheckinService> checkinServiceMock)
    {
        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock.SetupGet(x => x.SupportsCameraCapture).Returns(false);

        Services.AddSingleton(checkinServiceMock.Object);
        Services.AddSingleton(checkinProofServiceMock.Object);
    }

    private void RegisterSettingsServices()
    {
        var automatedBackupConfigurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        automatedBackupConfigurationServiceMock
            .Setup(x => x.IsSupported)
            .Returns(true);
        automatedBackupConfigurationServiceMock
            .Setup(x => x.GetIsEnabled())
            .Returns(false);

        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        reminderConfigurationServiceMock
            .Setup(x => x.GetIsEnabled())
            .Returns(false);
        reminderConfigurationServiceMock
            .Setup(x => x.GetTimeLocal())
            .Returns(new TimeOnly(21, 0));

        var oneDriveAuthServiceMock = new Mock<IOneDriveAuthService>();
        oneDriveAuthServiceMock
            .Setup(x => x.GetAuthStateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OneDriveAuthState
            {
                IsPlatformSupported = true,
                IsConfigured = true,
                IsConnected = false
            });

        var backupNotificationPermissionServiceMock = new Mock<IBackupNotificationPermissionService>();
        backupNotificationPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var reminderNotificationPermissionCoordinatorMock = new Mock<IReminderNotificationPermissionCoordinator>();
        reminderNotificationPermissionCoordinatorMock
            .Setup(x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var appVersionInfoServiceMock = new Mock<IAppVersionInfoService>();
        appVersionInfoServiceMock
            .Setup(x => x.GetCurrent())
            .Returns(new AppVersionInfo
            {
                DisplayVersion = "1.0.0",
                BuildNumber = "1"
            });

        Services.AddSingleton(automatedBackupConfigurationServiceMock.Object);
        Services.AddSingleton(reminderConfigurationServiceMock.Object);
        Services.AddSingleton(Mock.Of<IDatabaseExportService>());
        Services.AddSingleton(Mock.Of<IDiagnosticsExportService>());
        Services.AddSingleton(Mock.Of<IDatabaseShareService>());
        Services.AddSingleton(Mock.Of<IDiagnosticsShareService>());
        Services.AddSingleton(Mock.Of<IDatabaseImportFilePicker>());
        Services.AddSingleton(Mock.Of<IDatabaseImportService>());
        Services.AddSingleton(Mock.Of<IManualBackupCompletionNotifier>());
        Services.AddSingleton(backupNotificationPermissionServiceMock.Object);
        Services.AddSingleton(reminderNotificationPermissionCoordinatorMock.Object);
        Services.AddSingleton(appVersionInfoServiceMock.Object);
        Services.AddSingleton(oneDriveAuthServiceMock.Object);
        Services.AddSingleton(Mock.Of<IDialogService>());
    }

    #endregion
}
