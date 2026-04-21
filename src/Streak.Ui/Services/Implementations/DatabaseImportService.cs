using System.IO.Compression;

namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseImportService(
    IAppStoragePathService appStoragePathService,
    ICheckinProofFileStore checkinProofFileStore,
    SqliteDatabaseSchemaUpgrader sqliteDatabaseSchemaUpgrader,
    ILogger<DatabaseImportService> logger)
    : IDatabaseImportService
{
    private static readonly string[] DatabaseArtifactSuffixes = ["", "-journal", "-shm", "-wal"];
    private static readonly char[] PathSeparatorChars = ['\\', '/'];

    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;
    private readonly ICheckinProofFileStore _checkinProofFileStore = checkinProofFileStore;
    private readonly ILogger<DatabaseImportService> _logger = logger;
    private readonly SqliteDatabaseSchemaUpgrader _sqliteDatabaseSchemaUpgrader = sqliteDatabaseSchemaUpgrader;

    public async Task ImportDatabaseAsync(FileResult backupFile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(backupFile);

        var databasePath = _appStoragePathService.DatabasePath;
        var workingDirectoryPath = _appStoragePathService.ExportDirectoryPath;
        var candidateImportPath = CreateWorkingFilePath(workingDirectoryPath, "candidate", ".bin");
        var extractedArchiveDirectoryPath = CreateWorkingDirectoryPath(workingDirectoryPath, "extracted");
        var candidateBackupPath = Path.Combine(extractedArchiveDirectoryPath, DataBackupArchiveUtility.DatabaseEntryName);
        var rollbackDirectoryPath = CreateWorkingDirectoryPath(workingDirectoryPath, "rollback");
        var rollbackProofsDirectoryPath = Path.Combine(rollbackDirectoryPath, DataBackupArchiveUtility.CheckinProofsEntryRootName);
        var extractedProofsDirectoryPath = Path.Combine(extractedArchiveDirectoryPath, DataBackupArchiveUtility.CheckinProofsEntryRootName);

        try
        {
            await CopyBackupToWorkingFileAsync(backupFile, candidateImportPath, cancellationToken);

            var importSourceKind = DetectImportSourceKind(backupFile, candidateImportPath);

            if (importSourceKind == ImportSourceKind.Archive)
            {
                await ImportArchiveAsync(
                    candidateImportPath,
                    candidateBackupPath,
                    databasePath,
                    extractedArchiveDirectoryPath,
                    extractedProofsDirectoryPath,
                    rollbackDirectoryPath,
                    rollbackProofsDirectoryPath,
                    cancellationToken);
            }
            else
            {
                await ImportDatabaseFileAsync(
                    candidateImportPath,
                    databasePath,
                    rollbackDirectoryPath,
                    cancellationToken);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database import failed for {DatabasePath}.", databasePath);
            throw;
        }
        finally
        {
            DeleteIfExists(candidateImportPath);
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
        string candidateImportPath,
        CancellationToken cancellationToken)
    {
        await using var sourceStream = await backupFile.OpenReadAsync();
        if (sourceStream.CanSeek && sourceStream.Length == 0)
            throw new InvalidDataException("The selected backup file is empty.");

        await using var destinationStream = File.Create(candidateImportPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    private async Task ImportArchiveAsync(
        string candidateArchivePath,
        string candidateBackupPath,
        string databasePath,
        string extractedArchiveDirectoryPath,
        string extractedProofsDirectoryPath,
        string rollbackDirectoryPath,
        string rollbackProofsDirectoryPath,
        CancellationToken cancellationToken)
    {
        await ExtractBackupArchiveAsync(candidateArchivePath, extractedArchiveDirectoryPath, cancellationToken);
        await ValidateBackupAsync(candidateBackupPath, cancellationToken);

        BackupLiveDatabaseArtifacts(databasePath, rollbackDirectoryPath);
        await BackupLiveCheckinProofArtifactsAsync(rollbackProofsDirectoryPath, cancellationToken);

        try
        {
            ReplaceLiveDatabase(databasePath, candidateBackupPath);
            await ReplaceLiveCheckinProofArtifactsAsync(extractedProofsDirectoryPath, cancellationToken);
            _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(databasePath);
            await ValidateBackupAsync(databasePath, cancellationToken);
        }
        catch
        {
            RestoreLiveDatabaseArtifacts(databasePath, rollbackDirectoryPath);
            await RestoreLiveCheckinProofArtifactsAsync(rollbackProofsDirectoryPath, cancellationToken);
            throw;
        }
    }

    private async Task ImportDatabaseFileAsync(
        string candidateDatabasePath,
        string databasePath,
        string rollbackDirectoryPath,
        CancellationToken cancellationToken)
    {
        await ValidateBackupAsync(candidateDatabasePath, cancellationToken);

        BackupLiveDatabaseArtifacts(databasePath, rollbackDirectoryPath);

        try
        {
            ReplaceLiveDatabase(databasePath, candidateDatabasePath);
            _sqliteDatabaseSchemaUpgrader.UpgradeIfNeeded(databasePath);
            await ReconcileUnavailableProofReferencesAsync(
                databasePath,
                cancellationToken);
            await ValidateBackupAsync(databasePath, cancellationToken);
        }
        catch
        {
            RestoreLiveDatabaseArtifacts(databasePath, rollbackDirectoryPath);
            throw;
        }
    }

    private static ImportSourceKind DetectImportSourceKind(FileResult backupFile, string candidateImportPath)
    {
        var fileExtension = ResolveNormalizedFileExtension(backupFile);
        if (string.Equals(fileExtension, ".zip", StringComparison.OrdinalIgnoreCase))
            return ImportSourceKind.Archive;

        if (string.Equals(fileExtension, ".db", StringComparison.OrdinalIgnoreCase))
            return ImportSourceKind.Database;

        Span<byte> fileHeaderBytes = stackalloc byte[16];
        using var fileStream = File.OpenRead(candidateImportPath);
        var bytesRead = fileStream.Read(fileHeaderBytes);

        if (bytesRead >= 4
            && (fileHeaderBytes[..4].SequenceEqual("PK\u0003\u0004"u8)
                || fileHeaderBytes[..4].SequenceEqual("PK\u0005\u0006"u8)
                || fileHeaderBytes[..4].SequenceEqual("PK\u0007\u0008"u8)))
        {
            return ImportSourceKind.Archive;
        }

        if (bytesRead >= 16 && fileHeaderBytes[..16].SequenceEqual("SQLite format 3\0"u8))
            return ImportSourceKind.Database;

        throw new InvalidDataException("The selected file is not a recognizable Streak backup.");
    }

    private static string? ResolveNormalizedFileExtension(FileResult backupFile)
    {
        var fileName = !string.IsNullOrWhiteSpace(backupFile.FileName)
            ? backupFile.FileName
            : backupFile.FullPath;

        if (string.IsNullOrWhiteSpace(fileName))
            return null;

        var lastSeparatorIndex = fileName.AsSpan().LastIndexOfAny(PathSeparatorChars);
        var normalizedFileName = lastSeparatorIndex >= 0
            ? fileName[(lastSeparatorIndex + 1)..]
            : fileName;

        return Path.GetExtension(normalizedFileName);
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

    private async Task ReconcileUnavailableProofReferencesAsync(
        string databasePath,
        CancellationToken cancellationToken)
    {
        var unavailableReferencedProofPaths = await DataBackupArchiveUtility.GetUnavailableReferencedProofPathsAsync(
            databasePath,
            _checkinProofFileStore,
            cancellationToken);

        if (unavailableReferencedProofPaths.Count == 0)
            return;

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWrite,
            Pooling = false
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqliteTransaction)await connection.BeginTransactionAsync(cancellationToken);

        foreach (var unavailableReferencedProofPath in unavailableReferencedProofPaths)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText =
                """
                UPDATE Checkins
                SET ProofImageUri = NULL,
                    ProofImageDisplayName = NULL,
                    ProofImageSizeBytes = NULL,
                    ProofImageModifiedOn = NULL
                WHERE ProofImageUri IS NOT NULL
                  AND LENGTH(TRIM(ProofImageUri)) > 0
                  AND TRIM(ProofImageUri) = $proofImageUri;
                """;
            command.Parameters.AddWithValue("$proofImageUri", unavailableReferencedProofPath);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);

        _logger.LogWarning(
            "Direct database restore cleared {UnavailableProofFileCount} unavailable picture proof reference(s) for {DatabasePath}: {@UnavailableProofPaths}",
            unavailableReferencedProofPaths.Count,
            databasePath,
            unavailableReferencedProofPaths);
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

    private async Task BackupLiveCheckinProofArtifactsAsync(string rollbackProofsDirectoryPath, CancellationToken cancellationToken)
    {
        DeleteDirectoryIfExists(rollbackProofsDirectoryPath);
        Directory.CreateDirectory(rollbackProofsDirectoryPath);

        var proofImageUris = await _checkinProofFileStore.GetAllProofImageUrisAsync(cancellationToken);
        foreach (var proofImageUri in proofImageUris)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var destinationPath = CheckinProofPathUtility.GetAbsolutePath(
                rollbackProofsDirectoryPath,
                proofImageUri);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            await using var sourceStream = await _checkinProofFileStore.OpenReadAsync(proofImageUri, cancellationToken);
            await using var destinationStream = File.Create(destinationPath);
            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        }
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

    private async Task ReplaceLiveCheckinProofArtifactsAsync(
        string extractedProofsDirectoryPath,
        CancellationToken cancellationToken)
    {
        await _checkinProofFileStore.DeleteAllAsync(cancellationToken);

        if (!Directory.Exists(extractedProofsDirectoryPath))
            return;

        await ImportCheckinProofArtifactsFromDirectoryAsync(extractedProofsDirectoryPath, cancellationToken);
    }

    private async Task RestoreLiveCheckinProofArtifactsAsync(
        string rollbackProofsDirectoryPath,
        CancellationToken cancellationToken)
    {
        await ReplaceLiveCheckinProofArtifactsAsync(rollbackProofsDirectoryPath, cancellationToken);
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

    private async Task ImportCheckinProofArtifactsFromDirectoryAsync(
        string sourceDirectoryPath,
        CancellationToken cancellationToken)
    {
        foreach (var sourceFilePath in Directory.GetFiles(sourceDirectoryPath, "*", SearchOption.AllDirectories))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = Path
                .GetRelativePath(sourceDirectoryPath, sourceFilePath)
                .Replace(Path.DirectorySeparatorChar, '/');

            await using var sourceStream = File.OpenRead(sourceFilePath);
            await _checkinProofFileStore.SaveAsync(
                relativePath,
                sourceStream,
                CheckinProofFileUtility.GetMimeType(Path.GetExtension(sourceFilePath)),
                cancellationToken);
        }
    }

    private enum ImportSourceKind
    {
        Archive,
        Database
    }

    #endregion
}
