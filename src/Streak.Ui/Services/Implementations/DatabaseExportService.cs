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

        var backupFilePath = CreateBackupFilePath(_appStoragePathService.ExportDirectoryPath);

        try
        {
            await CreateBackupAsync(sourceDatabasePath, backupFilePath, cancellationToken);

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
            DeleteBackupIfExists(backupFilePath);
        }
    }

    private static string CreateBackupFilePath(string exportDirectoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportDirectoryPath);

        Directory.CreateDirectory(exportDirectoryPath);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var fileName = $"streak-backup-{timestamp}.db";

        return Path.Combine(exportDirectoryPath, fileName);
    }

    private static async Task CreateBackupAsync(
        string sourceDatabasePath,
        string backupFilePath,
        CancellationToken cancellationToken)
    {
        DeleteBackupIfExists(backupFilePath);

        var sourceConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = sourceDatabasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        var destinationConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = backupFilePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ToString();

        await using var sourceConnection = new SqliteConnection(sourceConnectionString);
        await using var destinationConnection = new SqliteConnection(destinationConnectionString);

        await sourceConnection.OpenAsync(cancellationToken);
        await destinationConnection.OpenAsync(cancellationToken);

        sourceConnection.BackupDatabase(destinationConnection);
    }

    private static void DeleteBackupIfExists(string backupFilePath)
    {
        if (File.Exists(backupFilePath))
            File.Delete(backupFilePath);
    }
}