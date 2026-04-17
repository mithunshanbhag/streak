namespace Streak.Ui.Services.Interfaces;

public interface IDiagnosticsExportService
{
    /// <summary>
    ///     Packages recent diagnostics artifacts into a support bundle and exports it using the current platform flow.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<DiagnosticsExportResult> ExportDiagnosticsAsync(CancellationToken cancellationToken = default);
}
