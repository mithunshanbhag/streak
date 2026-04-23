namespace Streak.Ui.Services.Implementations;

public sealed class DiagnosticsExportService(
    IAppStoragePathService appStoragePathService,
    IDiagnosticsExportFileSaver diagnosticsExportFileSaver,
    TimeProvider timeProvider,
    ILogger<DiagnosticsExportService> logger)
    : IDiagnosticsExportService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IDiagnosticsExportFileSaver _diagnosticsExportFileSaver = diagnosticsExportFileSaver;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<DiagnosticsExportService> _logger = logger;

    public async Task<DiagnosticsExportResult> ExportDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var bundleFilePath = DiagnosticsBundleUtility.CreateBundleFilePath(
            _appStoragePathService.ExportDirectoryPath,
            _timeProvider);

        try
        {
            await DiagnosticsBundleUtility.CreateBundleAsync(
                _appStoragePathService.DiagnosticsDirectoryPath,
                bundleFilePath,
                _timeProvider,
                cancellationToken);

            return await _diagnosticsExportFileSaver.SaveBundleAsync(
                bundleFilePath,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Diagnostics export failed for {DiagnosticsDirectoryPath}.",
                _appStoragePathService.DiagnosticsDirectoryPath);
            throw;
        }
        finally
        {
            DiagnosticsBundleUtility.DeleteBundleIfExists(bundleFilePath);
        }
    }
}
