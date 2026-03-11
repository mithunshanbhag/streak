using Microsoft.Data.Sqlite;

namespace Streak.Ui.Misc.Startup;

public sealed class SqliteDatabaseBootstrapper(ILogger<SqliteDatabaseBootstrapper> logger)
{
    public const string DatabaseFileName = "streak.local.db";
    public const string SchemaAssetName = "restore-db.sql";

    public static string GetDatabasePath()
    {
        return Path.Combine(FileSystem.Current.AppDataDirectory, DatabaseFileName);
    }

    public static string GetConnectionString()
    {
        return new SqliteConnectionStringBuilder
        {
            DataSource = GetDatabasePath()
        }.ToString();
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var databasePath = GetDatabasePath();
        if (File.Exists(databasePath))
        {
            logger.LogDebug("Skipping SQLite bootstrap because the database already exists at {DatabasePath}.", databasePath);
            return;
        }

        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory))
        {
            Directory.CreateDirectory(databaseDirectory);
        }

        var tempDatabasePath = Path.Combine(
            databaseDirectory ?? FileSystem.Current.AppDataDirectory,
            $"{Path.GetFileNameWithoutExtension(DatabaseFileName)}.{Guid.NewGuid():N}{Path.GetExtension(DatabaseFileName)}");

        logger.LogInformation("Bootstrapping SQLite database at {DatabasePath}.", databasePath);

        try
        {
            var schemaScript = await LoadSchemaScriptAsync().ConfigureAwait(false);
            await ExecuteSchemaScriptAsync(tempDatabasePath, schemaScript, cancellationToken).ConfigureAwait(false);

            File.Move(tempDatabasePath, databasePath);

            logger.LogInformation("SQLite database bootstrap completed at {DatabasePath}.", databasePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SQLite database bootstrap failed for {DatabasePath}.", databasePath);
            DeleteDatabaseArtifacts(tempDatabasePath);
            throw;
        }
    }

    private static async Task<string> LoadSchemaScriptAsync()
    {
        await using var scriptStream = await FileSystem.Current.OpenAppPackageFileAsync(SchemaAssetName).ConfigureAwait(false);
        using var reader = new StreamReader(scriptStream);

        return await reader.ReadToEndAsync().ConfigureAwait(false);
    }

    private static async Task ExecuteSchemaScriptAsync(string databasePath, string schemaScript, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        }.ToString());

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = schemaScript;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static void DeleteDatabaseArtifacts(string databasePath)
    {
        DeleteIfExists(databasePath);
        DeleteIfExists($"{databasePath}-journal");
        DeleteIfExists($"{databasePath}-shm");
        DeleteIfExists($"{databasePath}-wal");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
