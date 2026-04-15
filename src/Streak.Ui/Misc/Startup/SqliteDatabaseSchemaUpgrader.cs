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

        var hasHabitDescriptionColumn = ColumnExists(connection, "Habits", "Description");
        var hasAutomatedBackupSettingsTable = TableExists(connection, AutomatedBackupConstants.SettingsTableName);
        var hasAutomatedBackupSettingsRow = hasAutomatedBackupSettingsTable &&
                                           RowExists(connection, AutomatedBackupConstants.SettingsTableName, AutomatedBackupConstants.SettingsRowId);

        if (hasHabitDescriptionColumn && hasAutomatedBackupSettingsTable && hasAutomatedBackupSettingsRow)
        {
            _logger.LogDebug("SQLite schema is already up to date for {DatabasePath}.", databasePath);
            return;
        }

        _logger.LogInformation("Applying SQLite schema upgrades to {DatabasePath}.", databasePath);

        using var transaction = connection.BeginTransaction();
        using var command = connection.CreateCommand();
        command.Transaction = transaction;

        if (!hasHabitDescriptionColumn)
        {
            command.CommandText = "ALTER TABLE Habits ADD COLUMN Description TEXT NULL;";
            command.ExecuteNonQuery();
        }

        if (!hasAutomatedBackupSettingsTable)
        {
            command.CommandText =
                $"""
                 CREATE TABLE IF NOT EXISTS {AutomatedBackupConstants.SettingsTableName} (
                     Id INTEGER NOT NULL,
                     IsEnabled INTEGER NOT NULL DEFAULT 0,
                     CONSTRAINT PK_{AutomatedBackupConstants.SettingsTableName} PRIMARY KEY (Id),
                     CONSTRAINT CK_{AutomatedBackupConstants.SettingsTableName}_IsEnabled CHECK (IsEnabled IN (0, 1))
                 ) STRICT;
                 """;
            command.ExecuteNonQuery();
        }

        if (!hasAutomatedBackupSettingsRow)
        {
            command.CommandText =
                $"""
                 INSERT INTO {AutomatedBackupConstants.SettingsTableName} (Id, IsEnabled)
                 VALUES ({AutomatedBackupConstants.SettingsRowId}, 0)
                 ON CONFLICT(Id) DO NOTHING;
                 """;
            command.ExecuteNonQuery();
        }

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

    private static bool TableExists(SqliteConnection connection, string tableName)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = $tableName;
            """;
        command.Parameters.AddWithValue("$tableName", tableName);

        var result = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        return result > 0;
    }

    private static bool RowExists(SqliteConnection connection, string tableName, int id)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);

        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT(*) FROM \"{tableName}\" WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);

        var result = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        return result > 0;
    }

    #endregion
}
