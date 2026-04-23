namespace Streak.Ui.UnitTests.Components;

using Streak.Ui.Models.ViewModels;
using Streak.Ui.Services.Interfaces;
using Streak.Ui.Components;

public sealed class AppRootTests : TestContext
{
    public AppRootTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        Services.AddLogging();
        Services.AddSingleton(Mock.Of<ILogger<AppRoot>>());
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

    #region Private Helper Methods

    private void RegisterHomeServices(Mock<ICheckinService> checkinServiceMock)
    {
        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock.SetupGet(x => x.SupportsCameraCapture).Returns(false);

        Services.AddSingleton(checkinServiceMock.Object);
        Services.AddSingleton(checkinProofServiceMock.Object);
    }

    #endregion
}
