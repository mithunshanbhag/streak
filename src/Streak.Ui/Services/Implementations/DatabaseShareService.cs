namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseShareService(
    IAppStoragePathService appStoragePathService,
    IBackupArchiveFactory backupArchiveFactory,
    IShare share,
    ILogger<DatabaseShareService> logger)
    : IDatabaseShareService
{
    private const string DataBackupMimeType = "application/zip";

    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IBackupArchiveFactory _backupArchiveFactory = backupArchiveFactory;
    private readonly IShare _share = share;
    private readonly ILogger<DatabaseShareService> _logger = logger;

    public bool CanShare => true;

    public async Task ShareDatabaseAsync(CancellationToken cancellationToken = default)
    {
        DataBackupArchiveUtility.DeleteCachedBackups(_appStoragePathService.ExportDirectoryPath);
        var backupArchive = await _backupArchiveFactory.CreateManualBackupAsync(cancellationToken);

        try
        {
            if (backupArchive.UnavailableReferencedProofPaths.Count > 0)
            {
                _logger.LogWarning(
                    "Database share skipped {UnavailableProofFileCount} unavailable picture proof reference(s): {@UnavailableProofPaths}",
                    backupArchive.UnavailableReferencedProofPaths.Count,
                    backupArchive.UnavailableReferencedProofPaths);
            }

            await _share.RequestAsync(new ShareFileRequest
            {
                Title = "Share data",
                File = new ShareFile(backupArchive.WorkingFilePath, DataBackupMimeType)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database share failed.");
            throw;
        }
    }
}
