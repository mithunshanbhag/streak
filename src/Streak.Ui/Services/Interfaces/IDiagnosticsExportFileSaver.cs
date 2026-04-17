namespace Streak.Ui.Services.Interfaces;

public interface IDiagnosticsExportFileSaver
{
    /// <summary>
    ///     Saves a generated diagnostics support bundle using the current platform's file export flow.
    /// </summary>
    /// <param name="bundleFilePath">The full path to the generated diagnostics bundle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<DiagnosticsExportResult> SaveBundleAsync(
        string bundleFilePath,
        CancellationToken cancellationToken = default);
}
