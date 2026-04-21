using System.IO.Compression;

namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseImportService(
    IAppStoragePathService appStoragePathService,
    SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader,
    ILogger<DatabaseImportService> logger)
    : IDatabaseImportService
{
    private static readonly string[] DatabaseArtifactSuffixes = ["", "-journal", "-shm", "-wal"];

    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly ILogger<DatabaseImportService> _logger = logger;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;

    public async Task ImportDatabaseAsync(FileResult backupFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(backupFile);

        var databasePath = _appStoragePathService.DatabasePath;
        var checkinProofsDirectoryPath = _appStoragePathService.CheckinProofsDirectoryPath;
        var workingDirectoryPath = _appStoragePathService.ExportDirectoryPath;
        var candidateArchivePath = CreateWorkingFilePath(workingDirectoryPath, "candidate", ".zip");
        var extractedArchiveDirectoryPath = CreateWorkingDirectoryPath(workingDirectoryPath, "extracted");
        var candidateBackupPath = Path.Combine(extractedArchiveDirectoryPath, DataBackupArchiveUtility.DatabaseEntryName);
        var rollbackDirectoryPath = CreateWorkingDirectoryPath(workingDirectoryPath, "rollback");
        var rollbackProofsDirectoryPath = Path.Combine(rollbackDirectoryPath, DataBackupArchiveUtility.CheckinProofsEntryRootName);
        var extractedProofsDirectoryPath = Path.Combine(extractedArchiveDirectoryPath, DataBackupArchiveUtility.CheckinProofsEntryRootName);

        try
        {
            await CopyBackupToWorkingFileAsync(backupFile, candidateArchivePath, cancellationToken);
            await ExtractBackupArchiveAsync(candidateArchivePath, extractedArchiveDirectoryPath, cancellationToken);
            await ValidateBackupAsync(candidateBackupPath, cancellationToken);

            BackupLiveDatabaseArtifacts(databasePath, rollbackDirectoryPath);
            BackupLiveCheckinProofArtifacts(checkinProofsDirectoryPath, rollbackProofsDirectoryPath);

            try
            {
                ReplaceLiveDatabase(databasePath, candidateBackupPath);
                ReplaceLiveCheckinProofArtifacts(checkinProofsDirectoryPath, extractedProofsDirectoryPath);
                _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(databasePath);
                await ValidateBackupAsync(databasePath, cancellationToken);
            }
            catch
            {
                RestoreLiveDatabaseArtifacts(databasePath, rollbackDirectoryPath);
                RestoreLiveCheckinProofArtifacts(checkinProofsDirectoryPath, rollbackProofsDirectoryPath);
                throw;
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database import failed for {DatabasePath}.", databasePath);
            throw;
        }
        finally
        {
            DeleteIfExists(candidateArchivePath);
            DeleteDirectoryIfExists(rollbackDirectoryPath);
            DeleteDirectoryIfExists(extractedArchiveDirectoryPath);
        }
    }

    #region Private Helper Methods

    private static string CreateWorkingFilePath(
        string workingDirectoryPath,
        string operationName,
        string fileExtension)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileExtension);

        Directory.CreateDirectory(workingDirectoryPath);

        return Path.Combine(
            workingDirectoryPath,
            $"streak-{operationName}-{Guid.NewGuid():N}{fileExtension}");
    }

    private static string CreateWorkingDirectoryPath(string workingDirectoryPath, string operationName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationName);

        Directory.CreateDirectory(workingDirectoryPath);

        return Path.Combine(
            workingDirectoryPath,
            $"streak-{operationName}-{Guid.NewGuid():N}");
    }

    private static async Task CopyBackupToWorkingFileAsync(
        FileResult backupFile,
        string candidateArchivePath,
        CancellationToken cancellationToken)
    {
        await using var sourceStream = await backupFile.OpenReadAsync();
        if (sourceStream.CanSeek && sourceStream.Length == 0)
            throw new InvalidDataException("The selected backup file is empty.");

        await using var destinationStream = File.Create(candidateArchivePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    private static async Task ExtractBackupArchiveAsync(
        string archivePath,
        string extractedArchiveDirectoryPath,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(archivePath))
            throw new InvalidDataException("The selected backup file could not be read.");

        DeleteDirectoryIfExists(extractedArchiveDirectoryPath);
        Directory.CreateDirectory(extractedArchiveDirectoryPath);

        using var archive = ZipFile.OpenRead(archivePath);
        var databaseEntry = archive.GetEntry(DataBackupArchiveUtility.DatabaseEntryName)
                            ?? throw new InvalidDataException("The selected backup archive is missing the database file.");

        foreach (var entry in archive.Entries)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalizedEntryPath = entry.FullName.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(normalizedEntryPath))
                continue;

            if (normalizedEntryPath.EndsWith("/", StringComparison.Ordinal))
                continue;

            if (!string.Equals(normalizedEntryPath, DataBackupArchiveUtility.DatabaseEntryName, StringComparison.Ordinal)
                && !normalizedEntryPath.StartsWith($"{DataBackupArchiveUtility.CheckinProofsEntryRootName}/", StringComparison.Ordinal))
            {
                continue;
            }

            var entryPathSegments = normalizedEntryPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (entryPathSegments.Any(entryPathSegment => entryPathSegment is "." or ".."))
                throw new InvalidDataException("The selected backup archive contains invalid paths.");

            var destinationPath = Path.Combine(
                [.. new[] { extractedArchiveDirectoryPath }, .. entryPathSegments]);
            var destinationDirectoryPath = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrWhiteSpace(destinationDirectoryPath))
                Directory.CreateDirectory(destinationDirectoryPath);

            await using var entryStream = entry.Open();
            await using var destinationStream = File.Create(destinationPath);
            await entryStream.CopyToAsync(destinationStream, cancellationToken);
        }

        if (!File.Exists(Path.Combine(extractedArchiveDirectoryPath, databaseEntry.FullName)))
            throw new InvalidDataException("The selected backup archive is missing the database file.");
    }

    private static async Task ValidateBackupAsync(string databasePath, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!File.Exists(databasePath))
            throw new InvalidDataException("The selected backup file could not be read.");

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await EnsureIntegrityCheckPassedAsync(connection, cancellationToken);
        await EnsureExpectedTableSchemaAsync(connection, "Habits", ["Id", "Name", "Emoji"], cancellationToken);
        await EnsureExpectedTableSchemaAsync(connection, "Checkins", ["HabitId", "CheckinDate"], cancellationToken);
    }

    private static async Task EnsureIntegrityCheckPassedAsync(SqliteConnection connection, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA integrity_check(1);";

        var result = await command.ExecuteScalarAsync(cancellationToken) as string;
        if (!string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("The selected file is not a valid SQLite backup.");
    }

    private static async Task EnsureExpectedTableSchemaAsync(
        SqliteConnection connection,
        string tableName,
        IReadOnlyCollection<string> expectedColumns,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName}\");";

        var actualColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken)) actualColumns.Add(reader.GetString(1));

        if (actualColumns.Count == 0 || expectedColumns.Any(expectedColumn => !actualColumns.Contains(expectedColumn)))
            throw new InvalidDataException("The selected file is not a recognizable Streak backup.");
    }

    private static void BackupLiveDatabaseArtifacts(string databasePath, string rollbackDirectoryPath)
    {
        SqliteConnection.ClearAllPools();
        Directory.CreateDirectory(rollbackDirectoryPath);

        foreach (var artifactPath in GetDatabaseArtifactPaths(databasePath))
        {
            if (!File.Exists(artifactPath))
                continue;

            var rollbackArtifactPath = GetRollbackArtifactPath(rollbackDirectoryPath, artifactPath);
            File.Copy(artifactPath, rollbackArtifactPath, true);
        }
    }

    private static void BackupLiveCheckinProofArtifacts(string checkinProofsDirectoryPath, string rollbackProofsDirectoryPath)
    {
        DeleteDirectoryIfExists(rollbackProofsDirectoryPath);

        if (!Directory.Exists(checkinProofsDirectoryPath))
            return;

        CopyDirectory(checkinProofsDirectoryPath, rollbackProofsDirectoryPath);
    }

    private static void ReplaceLiveDatabase(string databasePath, string candidateBackupPath)
    {
        SqliteConnection.ClearAllPools();

        var databaseDirectoryPath = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectoryPath))
            Directory.CreateDirectory(databaseDirectoryPath);

        DeleteDatabaseArtifacts(databasePath);
        File.Copy(candidateBackupPath, databasePath, true);
    }

    private static void RestoreLiveDatabaseArtifacts(string databasePath, string rollbackDirectoryPath)
    {
        SqliteConnection.ClearAllPools();
        DeleteDatabaseArtifacts(databasePath);

        foreach (var artifactPath in GetDatabaseArtifactPaths(databasePath))
        {
            var rollbackArtifactPath = GetRollbackArtifactPath(rollbackDirectoryPath, artifactPath);
            if (File.Exists(rollbackArtifactPath))
                File.Copy(rollbackArtifactPath, artifactPath, true);
        }
    }

    private static void ReplaceLiveCheckinProofArtifacts(
        string checkinProofsDirectoryPath,
        string extractedProofsDirectoryPath)
    {
        DeleteDirectoryIfExists(checkinProofsDirectoryPath);

        if (!Directory.Exists(extractedProofsDirectoryPath))
            return;

        CopyDirectory(extractedProofsDirectoryPath, checkinProofsDirectoryPath);
    }

    private static void RestoreLiveCheckinProofArtifacts(
        string checkinProofsDirectoryPath,
        string rollbackProofsDirectoryPath)
    {
        DeleteDirectoryIfExists(checkinProofsDirectoryPath);

        if (!Directory.Exists(rollbackProofsDirectoryPath))
            return;

        CopyDirectory(rollbackProofsDirectoryPath, checkinProofsDirectoryPath);
    }

    private static IEnumerable<string> GetDatabaseArtifactPaths(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        return DatabaseArtifactSuffixes.Select(databaseArtifactSuffix => $"{databasePath}{databaseArtifactSuffix}");
    }

    private static string GetRollbackArtifactPath(string rollbackDirectoryPath, string sourceArtifactPath)
    {
        return Path.Combine(rollbackDirectoryPath, Path.GetFileName(sourceArtifactPath));
    }

    private static void DeleteDatabaseArtifacts(string databasePath)
    {
        foreach (var artifactPath in GetDatabaseArtifactPaths(databasePath))
            DeleteIfExists(artifactPath);
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }

    private static void DeleteDirectoryIfExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath))
            Directory.Delete(directoryPath, true);
    }

    private static void CopyDirectory(string sourceDirectoryPath, string destinationDirectoryPath)
    {
        Directory.CreateDirectory(destinationDirectoryPath);

        foreach (var sourceFilePath in Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectoryPath, sourceFilePath);
            var destinationFilePath = Path.Combine(destinationDirectoryPath, relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);
            File.Copy(sourceFilePath, destinationFilePath, true);
        }
    }

    #endregion
}
