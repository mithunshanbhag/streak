namespace Streak.Ui.UnitTests.Services;

public sealed class ExternalUrlLauncherTests
{
    #region Negative tests

    [Fact]
    public async Task OpenAsync_ShouldThrow_WhenUrlIsBlank()
    {
        var sut = new ExternalUrlLauncher();

        var act = () => sut.OpenAsync(" ");

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task OpenAsync_ShouldThrow_WhenCancellationIsRequested()
    {
        var sut = new ExternalUrlLauncher();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var act = () => sut.OpenAsync(UrlConstants.GitHubRepo, cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion
}
