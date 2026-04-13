namespace Streak.Ui.Misc.Startup;

public sealed class SqliteDatabaseSchemaUpgrader(ILogger<SqliteDatabaseSchemaUpgrader> logger)
{
    private readonly ILogger<SqliteDatabaseSchemaUpgrader> _logger = logger;

    public void UpgradeIfNeeded(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!File.Exists(databasePath))
            throw new FileNotFoundException("The SQLite database file was not found.", databasePath);

        SqliteConnection.ClearAllPools();

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false
        }.ToString();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        if (ColumnExists(connection, "Habits", "Description"))
        {
            _logger.LogDebug("SQLite schema is already up to date for {DatabasePath}.", databasePath);
            return;
        }

        _logger.LogInformation("Applying SQLite schema upgrades to {DatabasePath}.", databasePath);

        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = "ALTER TABLE Habits ADD COLUMN Description TEXT NULL;";
        command.ExecuteNonQuery();
        transaction.Commit();

        _logger.LogInformation("SQLite schema upgrades completed for {DatabasePath}.", databasePath);
    }

    #region Private Helper Methods

    private static bool ColumnExists(SqliteConnection connection, string tableName, string columnName)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);

        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName}\");";

        using var reader = command.ExecuteReader();
        while (reader.Read())
            if (string.Equals(reader.GetString(1), columnName, StringComparison.OrdinalIgnoreCase))
                return true;

        return false;
    }

    #endregion
}