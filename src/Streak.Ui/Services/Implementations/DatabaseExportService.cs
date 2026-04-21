namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseExportService(
    IAppStoragePathService appStoragePathService,
    IDatabaseExportFileSaver databaseExportFileSaver,
    ILogger<DatabaseExportService> logger)
    : IDatabaseExportService
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly IDatabaseExportFileSaver _databaseExportFileSaver = databaseExportFileSaver;
    private readonly ILogger<DatabaseExportService> _logger = logger;

    public async Task<DatabaseExportResult> ExportDatabaseAsync(CancellationToken cancellationToken = default)
    {
        var sourceDatabasePath = _appStoragePathService.DatabasePath;
        if (!File.Exists(sourceDatabasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", sourceDatabasePath);

        var backupFilePath = DataBackupArchiveUtility.CreateBackupFilePath(_appStoragePathService.ExportDirectoryPath);

        try
        {
            await DataBackupArchiveUtility.CreateBackupAsync(
                sourceDatabasePath,
                _appStoragePathService.CheckinProofsDirectoryPath,
                backupFilePath,
                cancellationToken);

            return await _databaseExportFileSaver.SaveBackupAsync(
                backupFilePath,
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database export failed for {DatabasePath}.", sourceDatabasePath);
            throw;
        }
        finally
        {
            DataBackupArchiveUtility.DeleteBackupIfExists(backupFilePath);
        }
    }
}
