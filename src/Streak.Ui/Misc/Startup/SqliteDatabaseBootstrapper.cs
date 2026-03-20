namespace Streak.Ui.Misc.Startup;

public sealed class SqliteDatabaseBootstrapper(ILogger<SqliteDatabaseBootstrapper> logger)
{
    public static string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();

    private static string DatabasePath => Path.Combine(FileSystem.Current.AppDataDirectory, AppConstants.DatabaseFileName);

    public async Task EnsureDbExistsAsync(CancellationToken cancellationToken = default)
    {
        var databasePath = DatabasePath;
        if (File.Exists(databasePath))
        {
            logger.LogDebug("Skipping SQLite bootstrap because the database already exists at {DatabasePath}.", databasePath);
            return;
        }

        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory)) Directory.CreateDirectory(databaseDirectory);

        var tempDatabasePath = Path.Combine(
            databaseDirectory ?? FileSystem.Current.AppDataDirectory,
            $"{Path.GetFileNameWithoutExtension(AppConstants.DatabaseFileName)}.{Guid.NewGuid():N}{Path.GetExtension(AppConstants.DatabaseFileName)}");

        logger.LogInformation("Bootstrapping SQLite database at {DatabasePath}.", databasePath);

        try
        {
            var schemaScript = await LoadSchemaScriptAsync();

            await ExecuteSchemaScriptAsync(tempDatabasePath, schemaScript, cancellationToken);

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

    #region Private Helper Methods

    private static async Task<string> LoadSchemaScriptAsync()
    {
        await using var scriptStream = await FileSystem.Current.OpenAppPackageFileAsync(AppConstants.SchemaAssetName);

        using var reader = new StreamReader(scriptStream);

        return await reader.ReadToEndAsync();
    }

    private static async Task ExecuteSchemaScriptAsync(string databasePath, string schemaScript, CancellationToken cancellationToken)
    {
        await using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        {
            DataSource = databasePath
        }.ToString());

        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText = schemaScript;

        await command.ExecuteNonQueryAsync(cancellationToken);
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
        if (File.Exists(path)) File.Delete(path);
    }

    #endregion
}