namespace Streak.Ui.Services.Implementations;

internal static class DatabaseBackupFileUtility
{
    private const string ManualBackupFilePattern = "streak-backup-*.db";

    private const string AutomatedBackupFilePattern = "streak-auto-backup-*.db";

    internal static string CreateBackupFilePath(string exportDirectoryPath)
    {
        return CreateBackupFilePath(exportDirectoryPath, "streak-backup");
    }

    internal static string CreateAutomatedBackupFilePath(string exportDirectoryPath)
    {
        return CreateBackupFilePath(exportDirectoryPath, "streak-auto-backup");
    }

    internal static string CreateBackupFilePath(string exportDirectoryPath, string filePrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportDirectoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePrefix);

        Directory.CreateDirectory(exportDirectoryPath);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var fileName = $"{filePrefix}-{timestamp}.db";

        return Path.Combine(exportDirectoryPath, fileName);
    }

    internal static async Task CreateBackupAsync(
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

    internal static void DeleteBackupIfExists(string backupFilePath)
    {
        if (File.Exists(backupFilePath))
            File.Delete(backupFilePath);
    }

    internal static void DeleteCachedBackups(string exportDirectoryPath)
    {
        DeleteCachedBackups(exportDirectoryPath, ManualBackupFilePattern);
    }

    internal static void DeleteAutomatedBackups(string exportDirectoryPath)
    {
        DeleteCachedBackups(exportDirectoryPath, AutomatedBackupFilePattern);
    }

    private static void DeleteCachedBackups(string exportDirectoryPath, string backupFilePattern)
    {
        if (!Directory.Exists(exportDirectoryPath))
            return;

        foreach (var backupFilePath in Directory.GetFiles(exportDirectoryPath, backupFilePattern))
            File.Delete(backupFilePath);
    }
}
