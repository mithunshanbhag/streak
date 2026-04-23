namespace Streak.Ui.UnitTests.Services;

public sealed class AppInitializationServiceTests
{
    #region Positive tests

    [Fact]
    public async Task EnsureInitializedAsync_ShouldRunStartupWorkOnlyOnce_WhenCalledConcurrently()
    {
        var appStartupWorkServiceMock = new Mock<IAppStartupWorkService>();
        var loggerMock = new Mock<ILogger<AppInitializationService>>();
        var enteredStartupWork = new ManualResetEventSlim(false);
        var continueStartupWork = new ManualResetEventSlim(false);
        var startupInvocationCount = 0;

        appStartupWorkServiceMock
            .Setup(x => x.Execute())
            .Callback(() =>
            {
                Interlocked.Increment(ref startupInvocationCount);
                enteredStartupWork.Set();
                continueStartupWork.Wait(TimeSpan.FromSeconds(5));
            });

        var sut = new AppInitializationService(
            appStartupWorkServiceMock.Object,
            loggerMock.Object);

        var firstCall = sut.EnsureInitializedAsync();
        enteredStartupWork.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue();

        var secondCall = sut.EnsureInitializedAsync();

        await Task.Delay(50);
        startupInvocationCount.Should().Be(1);

        continueStartupWork.Set();

        await Task.WhenAll(firstCall, secondCall);

        appStartupWorkServiceMock.Verify(x => x.Execute(), Times.Once);
    }

    #endregion
}
