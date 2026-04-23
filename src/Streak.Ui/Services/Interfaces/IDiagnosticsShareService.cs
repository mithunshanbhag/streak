namespace Streak.Ui.Services.Interfaces;

public interface IDiagnosticsShareService
{
    /// <summary>
    /// Gets a value indicating whether diagnostics sharing is supported on the current platform.
    /// </summary>
    bool CanShare { get; }

    /// <summary>
    /// Creates a diagnostics support bundle and opens the operating system share flow for it.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task ShareDiagnosticsAsync(CancellationToken cancellationToken = default);
}
