namespace Streak.Ui.Services.Implementations;

public sealed class ExternalUrlLauncher : IExternalUrlLauncher
{
    public async Task OpenAsync(string url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);
        cancellationToken.ThrowIfCancellationRequested();

        await Browser.Default.OpenAsync(url, BrowserLaunchMode.External);
    }
}
