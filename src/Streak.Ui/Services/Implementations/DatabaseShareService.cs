namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseShareService(
    IAppStoragePathService appStoragePathService,
    ICheckinProofFileStore checkinProofFileStore,
    IShare share,
    ILogger<DatabaseShareService> logger)
    : IDatabaseShareService
{
    private const string DataBackupMimeType = "application/zip";

    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly ICheckinProofFileStore _checkinProofFileStore = checkinProofFileStore;
    private readonly IShare _share = share;
    private readonly ILogger<DatabaseShareService> _logger = logger;

    public bool CanShare => true;

    public async Task ShareDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var sourceDatabasePath = _appStoragePathService.DatabasePath;
        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", sourceDatabasePath);

        var exportDirectoryPath = _appStoragePathService.ExportDirectoryPath;
        DataBackupArchiveUtility.DeleteCachedBackups(exportDirectoryPath);

        var backupFilePath = DataBackupArchiveUtility.CreateBackupFilePath(exportDirectoryPath);

        try
        {
            var unavailableReferencedProofPaths = await DataBackupArchiveUtility.CreateBackupAsync(
                sourceDatabasePath,
                _checkinProofFileStore,
                backupFilePath,
                cancellationToken);

            if (unavailableReferencedProofPaths.Count > 0)
            {
                _logger.LogWarning(
                    "Database share skipped {UnavailableProofFileCount} unavailable picture proof reference(s) for {DatabasePath}: {@UnavailableProofPaths}",
                    unavailableReferencedProofPaths.Count,
                    sourceDatabasePath,
                    unavailableReferencedProofPaths);
            }

            await _share.RequestAsync(new ShareFileRequest
            {
                Title = "Share data",
                File = new ShareFile(backupFilePath, DataBackupMimeType)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database share failed for {DatabasePath}.", sourceDatabasePath);
            throw;
        }
    }
}
