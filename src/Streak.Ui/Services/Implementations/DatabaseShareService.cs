namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseShareService(
    IAppStoragePathService appStoragePathService,
    IShare share,
    ILogger<DatabaseShareService> logger)
    : IDatabaseShareService
{
    private const string DatabaseMimeType = "application/octet-stream";

    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IShare _share = share;
    private readonly ILogger<DatabaseShareService> _logger = logger;

    public bool CanShare => true;

    public async Task ShareDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var sourceDatabasePath = _appStoragePathService.DatabasePath;
        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", sourceDatabasePath);

        var exportDirectoryPath = _appStoragePathService.ExportDirectoryPath;
        DatabaseBackupFileUtility.DeleteCachedBackups(exportDirectoryPath);

        var backupFilePath = DatabaseBackupFileUtility.CreateBackupFilePath(exportDirectoryPath);

        try
        {
            await DatabaseBackupFileUtility.CreateBackupAsync(
                sourceDatabasePath,
                backupFilePath,
                cancellationToken);

            await _share.RequestAsync(new ShareFileRequest
            {
                Title = "Share DB",
                File = new ShareFile(backupFilePath, DatabaseMimeType)
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database share failed for {DatabasePath}.", sourceDatabasePath);
            throw;
        }
    }
}
