namespace Streak.Ui.UnitTests.Components.Layout;

using Microsoft.AspNetCore.Components;
using Streak.Core.Constants;
using Streak.Core.Models.Storage;
using Streak.Core.Services.Interfaces;
using Streak.Core.Models.ViewModels;
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
        RegisterHomeServices();
        RegisterSettingsServices();

        var cut = RenderRoutes();
        Services.GetRequiredService<NavigationManager>().NavigateTo(RouteConstants.Settings);

        cut.WaitForAssertion(() =>
        {
            cut.Find("[aria-label='Back to habits']");
            cut.Find(".app-bar-title-text").TextContent.Should().Be("Settings");
        });
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

        Services.AddSingleton(checkinServiceMock.Object);
    }

    private void RegisterSettingsServices()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = new Mock<IDatabaseShareService>();
        shareServiceMock.SetupGet(x => x.CanShare).Returns(false);

        var backupConfigurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        backupConfigurationServiceMock.SetupGet(x => x.IsSupported).Returns(true);
        backupConfigurationServiceMock.Setup(x => x.GetIsEnabled()).Returns(false);

        Services.AddSingleton(exportServiceMock.Object);
        Services.AddSingleton(diagnosticsExportServiceMock.Object);
        Services.AddSingleton(shareServiceMock.Object);
        Services.AddSingleton(backupConfigurationServiceMock.Object);
        Services.AddSingleton(Mock.Of<IDatabaseImportFilePicker>());
        Services.AddSingleton(Mock.Of<IDatabaseImportService>());
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

        Services.AddSingleton(habitServiceMock.Object);
        Services.AddSingleton(checkinServiceMock.Object);
    }

    private IRenderedComponent<Routes> RenderRoutes()
    {
        return RenderComponent<Routes>();
    }

    #endregion
}
