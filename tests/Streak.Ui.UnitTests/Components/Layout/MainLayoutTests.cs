namespace Streak.Ui.UnitTests.Components.Layout;

using Microsoft.AspNetCore.Components;
using Streak.Ui.Constants;
using Streak.Ui.Models.Storage;
using Streak.Ui.Models.ViewModels;
using Streak.Ui.Components;
using Streak.Ui.Components.Layout;

public sealed class MainLayoutTests : TestContext
{
    public MainLayoutTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void MainLayout_ShouldRenderSettingsTitle_AfterRouterNavigation()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo(RouteConstants.Settings);

        var cut = RenderMainLayoutWithTitle("Settings");

        cut.WaitForAssertion(() =>
        {
            cut.Find("[aria-label='Back to habits']");
            cut.Find(".app-bar-title-text").TextContent.Should().Be("Settings");
            cut.FindAll("[aria-label='Scroll to top of page']").Should().BeEmpty();
        });
    }

    [Fact]
    public void MainLayout_ShouldRenderScrollToTop_OnHomeRoute()
    {
        RegisterHomeServices();
        RegisterSettingsServices();

        var cut = RenderRoutes();

        cut.WaitForAssertion(() => { cut.Find("[aria-label='Scroll to top of page']"); });
    }

    [Fact]
    public void MainLayout_ShouldRenderGitHubRepositoryHref_OnHomeRoute()
    {
        var cut = RenderMainLayoutWithTitle("Settings");

        Services.GetRequiredService<NavigationManager>().NavigateTo(RouteConstants.Home);
        cut.Find("[aria-label='View source code on GitHub']")
            .GetAttribute("href")
            .Should()
            .Be(UrlConstants.GitHubRepo);
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void MainLayout_ShouldKeepEmojiVisibleAndEllipsizeOnlyHabitName_AfterRouterNavigation()
    {
        const string longHabitName = "Practice an intentionally very long nighttime meditation routine";

        RegisterHomeServices();
        RegisterHabitDetailsServices(
            new Habit
            {
                Id = 42,
                Name = longHabitName,
                Emoji = "🧘"
            });

        var cut = RenderRoutes();
        Services.GetRequiredService<NavigationManager>().NavigateTo(RouteConstants.GetHabitDetails(42));

        cut.WaitForAssertion(() =>
        {
            cut.Find("[aria-label='Back to habits']");
            cut.Find(".app-bar-title-emoji").TextContent.Should().Be("🧘");

            var titleText = cut.Find(".app-bar-title-text");
            titleText.TextContent.Should().Be(longHabitName);
            titleText.GetAttribute("style").Should().Contain("overflow: hidden");
            titleText.GetAttribute("style").Should().Contain("text-overflow: ellipsis");
            titleText.GetAttribute("style").Should().Contain("white-space: nowrap");
        });
    }

    #endregion

    #region Private Helper Methods

    private void RegisterHomeServices()
    {
        var checkinServiceMock = new Mock<ICheckinService>();
        checkinServiceMock
            .Setup(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HabitCheckinViewModel>());

        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock.SetupGet(x => x.SupportsCameraCapture).Returns(false);
        var postStartupPermissionRecoveryCoordinatorMock = new Mock<IPostStartupPermissionRecoveryCoordinator>();
        postStartupPermissionRecoveryCoordinatorMock
            .Setup(x => x.RecoverMissingPermissionsAfterHomepageRenderAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Services.AddSingleton(checkinServiceMock.Object);
        Services.AddSingleton(checkinProofServiceMock.Object);
        Services.AddSingleton(postStartupPermissionRecoveryCoordinatorMock.Object);
        Services.AddSingleton(TimeProvider.System);
    }

    private void RegisterSettingsServices()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var shareServiceMock = new Mock<IDatabaseShareService>();
        shareServiceMock.SetupGet(x => x.CanShare).Returns(false);

        var backupConfigurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        backupConfigurationServiceMock.SetupGet(x => x.IsSupported).Returns(true);
        backupConfigurationServiceMock.Setup(x => x.GetIsEnabled()).Returns(false);
        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        reminderConfigurationServiceMock.Setup(x => x.GetIsEnabled()).Returns(true);
        reminderConfigurationServiceMock.Setup(x => x.GetTimeLocal()).Returns(new TimeOnly(21, 0));
        var reminderNotificationPermissionCoordinatorMock = new Mock<IReminderNotificationPermissionCoordinator>();
        reminderNotificationPermissionCoordinatorMock
            .Setup(x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        var appVersionInfoServiceMock = new Mock<IAppVersionInfoService>();
        appVersionInfoServiceMock
            .Setup(x => x.GetCurrent())
            .Returns(new AppVersionInfo
            {
                DisplayVersion = "1.0",
                BuildNumber = "123"
            });

        Services.AddSingleton(exportServiceMock.Object);
        Services.AddSingleton(shareServiceMock.Object);
        Services.AddSingleton(backupConfigurationServiceMock.Object);
        Services.AddSingleton(reminderConfigurationServiceMock.Object);
        Services.AddSingleton(Mock.Of<IDatabaseImportFilePicker>());
        Services.AddSingleton(Mock.Of<IDatabaseImportService>());
        Services.AddSingleton(Mock.Of<IManualBackupCompletionNotifier>());
        Services.AddSingleton(reminderNotificationPermissionCoordinatorMock.Object);
        Services.AddSingleton(appVersionInfoServiceMock.Object);

        var backupNotificationPermissionServiceMock = new Mock<IBackupNotificationPermissionService>();
        backupNotificationPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        Services.AddSingleton(backupNotificationPermissionServiceMock.Object);
    }

    private void RegisterHabitDetailsServices(Habit habit)
    {
        var habitServiceMock = new Mock<IHabitService>();
        habitServiceMock
            .Setup(x => x.GetByIdAsync(habit.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(habit);

        var checkinServiceMock = new Mock<ICheckinService>();
        checkinServiceMock
            .Setup(x => x.GetCurrentStreakAsync(habit.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(12);
        checkinServiceMock
            .Setup(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<HabitCheckinViewModel>());
        checkinServiceMock
            .Setup(x => x.GetHistoryAsync(habit.Name, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Checkin>());

        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock.SetupGet(x => x.SupportsCameraCapture).Returns(false);
        var postStartupPermissionRecoveryCoordinatorMock = new Mock<IPostStartupPermissionRecoveryCoordinator>();
        postStartupPermissionRecoveryCoordinatorMock
            .Setup(x => x.RecoverMissingPermissionsAfterHomepageRenderAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Services.AddSingleton(habitServiceMock.Object);
        Services.AddSingleton(checkinServiceMock.Object);
        Services.AddSingleton(checkinProofServiceMock.Object);
        Services.AddSingleton(postStartupPermissionRecoveryCoordinatorMock.Object);
        Services.AddSingleton(TimeProvider.System);
    }

    private IRenderedComponent<Routes> RenderRoutes()
    {
        return RenderComponent<Routes>();
    }

    private IRenderedComponent<MainLayout> RenderMainLayoutWithTitle(string titleText)
    {
        RenderFragment body = builder =>
        {
            builder.OpenComponent<AppBarTitleScope>(0);
            builder.AddAttribute(1, nameof(AppBarTitleScope.TitleText), titleText);
            builder.CloseComponent();
        };

        return RenderComponent<MainLayout>(parameters => parameters.Add(component => component.Body, body));
    }

    #endregion
}
