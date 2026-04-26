namespace Streak.Ui.Services.Interfaces;

public interface IExternalUrlLauncher
{
    /// <summary>
    ///     Opens an absolute URL outside the app.
    /// </summary>
    /// <param name="url">The absolute URL to open.</param>
    /// <param name="cancellationToken">A token that can cancel the operation.</param>
    Task OpenAsync(string url, CancellationToken cancellationToken = default);
}
