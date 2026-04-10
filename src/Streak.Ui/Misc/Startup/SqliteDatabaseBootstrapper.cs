namespace Streak.Ui.Misc.Startup;

public sealed class SqliteDatabaseBootstrapper(ILogger<SqliteDatabaseBootstrapper> logger)
{
    public static string ConnectionString => new SqliteConnectionStringBuilder { DataSource = DatabasePath }.ToString();

    public static string DatabasePath => Path.Combine(FileSystem.Current.AppDataDirectory, AppConstants.DatabaseFileName);

    public void EnsureDbExists()
    {
        var databasePath = DatabasePath;
        if (File.Exists(databasePath))
        {
            logger.LogDebug("Skipping SQLite bootstrap because the database already exists at {DatabasePath}.", databasePath);
            return;
        }

        var databaseDirectory = Path.GetDirectoryName(databasePath);
        if (!string.IsNullOrWhiteSpace(databaseDirectory)) Directory.CreateDirectory(databaseDirectory);

        logger.LogInformation("Bootstrapping SQLite database at {DatabasePath}.", databasePath);

        try
        {
            var schemaScript = LoadSchemaScript();

            ExecuteSchemaScript(databasePath, schemaScript);

            logger.LogInformation("SQLite database bootstrap completed at {DatabasePath}.", databasePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SQLite database bootstrap failed for {DatabasePath}.", databasePath);

            DeleteDatabaseArtifacts(databasePath);

            throw;
        }
    }

    #region Private Helper Methods

    private static string LoadSchemaScript()
    {
        var manifestResourceStream = typeof(SqliteDatabaseBootstrapper).Assembly.GetManifestResourceStream(AppConstants.SchemaAssetName);

        using var scriptStream = manifestResourceStream
                                 ?? throw new InvalidOperationException($"SQLite schema resource '{AppConstants.SchemaAssetName}' was not found.");

        using var reader = new StreamReader(scriptStream);

        return reader.ReadToEnd();
    }

    private static void ExecuteSchemaScript(string databasePath, string schemaScript)
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false
        };

        var connectionString = connectionStringBuilder.ToString();

        using var connection = new SqliteConnection(connectionString);

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = schemaScript;

        command.ExecuteNonQuery();
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