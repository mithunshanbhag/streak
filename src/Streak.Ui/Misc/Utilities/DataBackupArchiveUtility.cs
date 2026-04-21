using System.IO.Compression;

namespace Streak.Ui.Misc.Utilities;

internal static class DataBackupArchiveUtility
{
    private const string ManualBackupFilePattern = "streak-data-backup-*.zip";

    private const string AutomatedBackupFilePattern = "streak-auto-data-backup-*.zip";

    internal const string DatabaseEntryName = "streak.db";

    internal const string CheckinProofsEntryRootName = "CheckinProofs";

    internal static string CreateBackupFilePath(string exportDirectoryPath)
    {
        return CreateBackupFilePath(exportDirectoryPath, "streak-data-backup");
    }

    internal static string CreateAutomatedBackupFilePath(string exportDirectoryPath)
    {
        return CreateBackupFilePath(exportDirectoryPath, "streak-auto-data-backup");
    }

    internal static string CreateBackupFilePath(string exportDirectoryPath, string filePrefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(exportDirectoryPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePrefix);

        Directory.CreateDirectory(exportDirectoryPath);

        var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var fileName = $"{filePrefix}-{timestamp}.zip";

        return Path.Combine(exportDirectoryPath, fileName);
    }

    internal static async Task<IReadOnlyList<string>> CreateBackupAsync(
        string sourceDatabasePath,
        ICheckinProofFileStore checkinProofFileStore,
        string backupFilePath,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDatabasePath);
        ArgumentNullException.ThrowIfNull(checkinProofFileStore);
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        DeleteBackupIfExists(backupFilePath);

        var workingDatabasePath = CreateWorkingDatabasePath(backupFilePath);

        try
        {
            await CreateDatabaseSnapshotAsync(sourceDatabasePath, workingDatabasePath, cancellationToken);

            var (referencedProofFiles, unavailableReferencedProofPaths) = await ScanReferencedProofFilesAsync(
                workingDatabasePath,
                checkinProofFileStore,
                cancellationToken);

            await CreateArchiveAsync(
                backupFilePath,
                workingDatabasePath,
                referencedProofFiles,
                checkinProofFileStore,
                cancellationToken);

            return unavailableReferencedProofPaths;
        }
        finally
        {
            DeleteBackupIfExists(workingDatabasePath);
        }
    }

    internal static async Task<IReadOnlyList<string>> GetUnavailableReferencedProofPathsAsync(
        string databasePath,
        ICheckinProofFileStore checkinProofFileStore,
        CancellationToken cancellationToken)
    {
        var (_, unavailableReferencedProofPaths) = await ScanReferencedProofFilesAsync(
            databasePath,
            checkinProofFileStore,
            cancellationToken);

        return unavailableReferencedProofPaths;
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

    private static string CreateWorkingDatabasePath(string backupFilePath)
    {
        var backupDirectoryPath = Path.GetDirectoryName(backupFilePath)
                                  ?? throw new InvalidOperationException("Unable to determine the backup working directory.");

        Directory.CreateDirectory(backupDirectoryPath);

        return Path.Combine(
            backupDirectoryPath,
            $"{Path.GetFileNameWithoutExtension(backupFilePath)}-database.db");
    }

    private static async Task CreateDatabaseSnapshotAsync(
        string sourceDatabasePath,
        string destinationDatabasePath,
        CancellationToken cancellationToken)
    {
        DeleteBackupIfExists(destinationDatabasePath);

        var sourceConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = sourceDatabasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        var destinationConnectionString = new SqliteConnectionStringBuilder
        {
            DataSource = destinationDatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false
        }.ToString();

        await using var sourceConnection = new SqliteConnection(sourceConnectionString);
        await using var destinationConnection = new SqliteConnection(destinationConnectionString);

        await sourceConnection.OpenAsync(cancellationToken);
        await destinationConnection.OpenAsync(cancellationToken);

        sourceConnection.BackupDatabase(destinationConnection);
    }

    private static async Task<(IReadOnlyList<ReferencedProofFile> ReferencedProofFiles, IReadOnlyList<string> UnavailableReferencedProofPaths)> ScanReferencedProofFilesAsync(
        string databasePath,
        ICheckinProofFileStore checkinProofFileStore,
        CancellationToken cancellationToken)
    {
        if (!await HasProofImageUriColumnAsync(databasePath, cancellationToken))
            return ([], []);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT DISTINCT TRIM(ProofImageUri)
            FROM Checkins
            WHERE ProofImageUri IS NOT NULL
              AND LENGTH(TRIM(ProofImageUri)) > 0
            ORDER BY 1;
            """;

        var referencedProofFiles = new List<ReferencedProofFile>();
        var unavailableReferencedProofPaths = new List<string>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var relativeProofPath = reader.GetString(0);
            if (!CheckinProofPathUtility.TryNormalizeRelativeProofPath(relativeProofPath, out var normalizedRelativeProofPath))
            {
                unavailableReferencedProofPaths.Add(relativeProofPath);
                continue;
            }

            if (!await checkinProofFileStore.ExistsAsync(normalizedRelativeProofPath, cancellationToken))
            {
                unavailableReferencedProofPaths.Add(relativeProofPath);
                continue;
            }

            referencedProofFiles.Add(new ReferencedProofFile
            {
                ProofImageUri = normalizedRelativeProofPath,
                ArchiveEntryPath = $"{CheckinProofsEntryRootName}/{normalizedRelativeProofPath}"
            });
        }

        return (referencedProofFiles, unavailableReferencedProofPaths);
    }

    private static async Task<bool> HasProofImageUriColumnAsync(string databasePath, CancellationToken cancellationToken)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadOnly,
            Pooling = false
        }.ToString();

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA table_info(\"Checkins\");";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            if (string.Equals(reader.GetString(1), "ProofImageUri", StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static async Task CreateArchiveAsync(
        string archiveFilePath,
        string databasePath,
        IReadOnlyCollection<ReferencedProofFile> referencedProofFiles,
        ICheckinProofFileStore checkinProofFileStore,
        CancellationToken cancellationToken)
    {
        await using var bundleStream = new FileStream(
            archiveFilePath,
            FileMode.CreateNew,
            FileAccess.ReadWrite,
            FileShare.None);
        using var zipArchive = new ZipArchive(bundleStream, ZipArchiveMode.Create, leaveOpen: false);

        await AddFileAsync(zipArchive, databasePath, DatabaseEntryName, cancellationToken);

        foreach (var referencedProofFile in referencedProofFiles)
        {
            await using var sourceStream = await checkinProofFileStore.OpenReadAsync(
                referencedProofFile.ProofImageUri,
                cancellationToken);
            await AddStreamAsync(
                zipArchive,
                sourceStream,
                referencedProofFile.ArchiveEntryPath,
                cancellationToken);
        }
    }

    private static async Task AddFileAsync(
        ZipArchive zipArchive,
        string sourceFilePath,
        string archiveEntryPath,
        CancellationToken cancellationToken)
    {
        await using var sourceStream = File.OpenRead(sourceFilePath);
        await AddStreamAsync(zipArchive, sourceStream, archiveEntryPath, cancellationToken);
    }

    private static async Task AddStreamAsync(
        ZipArchive zipArchive,
        Stream sourceStream,
        string archiveEntryPath,
        CancellationToken cancellationToken)
    {
        var entry = zipArchive.CreateEntry(archiveEntryPath, CompressionLevel.Optimal);
        await using var entryStream = entry.Open();
        await sourceStream.CopyToAsync(entryStream, cancellationToken);
    }

    private static void DeleteCachedBackups(string exportDirectoryPath, string backupFilePattern)
    {
        if (!Directory.Exists(exportDirectoryPath))
            return;

        foreach (var backupFilePath in Directory.GetFiles(exportDirectoryPath, backupFilePattern))
            File.Delete(backupFilePath);
    }

    private sealed class ReferencedProofFile
    {
        public required string ProofImageUri { get; init; }

        public required string ArchiveEntryPath { get; init; }
    }
}
