namespace Streak.Ui.Services.Implementations;

public sealed class DiagnosticsShareService(
    IAppStoragePathService appStoragePathService,
    TimeProvider timeProvider,
    IShare share,
    ILogger<DiagnosticsShareService> logger)
    : IDiagnosticsShareService
{
    private const string DiagnosticsBundleMimeType = "application/zip";

    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly IShare _share = share;
    private readonly ILogger<DiagnosticsShareService> _logger = logger;

    public bool CanShare => true;

    public async Task ShareDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        var exportDirectoryPath = _appStoragePathService.ExportDirectoryPath;
        DiagnosticsBundleUtility.DeleteCachedBundles(exportDirectoryPath);

        var bundleFilePath = DiagnosticsBundleUtility.CreateBundleFilePath(
            exportDirectoryPath,
            _timeProvider);

        try
        {
            await DiagnosticsBundleUtility.CreateBundleAsync(
                _appStoragePathService.DiagnosticsDirectoryPath,
                bundleFilePath,
                _timeProvider,
                cancellationToken);

            await _share.RequestAsync(new ShareFileRequest
            {
                Title = "Share diagnostic logs",
                File = new ShareFile(bundleFilePath, DiagnosticsBundleMimeType)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Diagnostics share failed for {DiagnosticsDirectoryPath}.",
                _appStoragePathService.DiagnosticsDirectoryPath);
            throw;
        }
    }
}
