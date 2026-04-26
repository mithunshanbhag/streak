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
        var hasCheckinNotesColumn = ColumnExists(connection, "Checkins", "Notes");
        var hasCheckinProofImageUriColumn = ColumnExists(connection, "Checkins", "ProofImageUri");
        var hasCheckinProofImageDisplayNameColumn = ColumnExists(connection, "Checkins", "ProofImageDisplayName");
        var hasCheckinProofImageSizeBytesColumn = ColumnExists(connection, "Checkins", "ProofImageSizeBytes");
        var hasCheckinProofImageModifiedOnColumn = ColumnExists(connection, "Checkins", "ProofImageModifiedOn");
        var hasAutomatedBackupSettingsTable = TableExists(connection, AutomatedBackupConstants.SettingsTableName);
        var hasAutomatedBackupCloudEnabledColumn = hasAutomatedBackupSettingsTable
                                                   && ColumnExists(connection, AutomatedBackupConstants.SettingsTableName, AutomatedBackupConstants.CloudEnabledColumnName);
        var hasAutomatedBackupSettingsRow = hasAutomatedBackupSettingsTable
                                             && RowExists(connection, AutomatedBackupConstants.SettingsTableName, AutomatedBackupConstants.SettingsRowId);
        var hasReminderSettingsTable = TableExists(connection, ReminderConstants.SettingsTableName);
        var hasReminderTimeLocalColumn = hasReminderSettingsTable
                                         && ColumnExists(connection, ReminderConstants.SettingsTableName, ReminderConstants.TimeLocalColumnName);
        var hasReminderSettingsRow = hasReminderSettingsTable
                                     && RowExists(connection, ReminderConstants.SettingsTableName, ReminderConstants.SettingsRowId);

        if (hasHabitDescriptionColumn &&
            hasCheckinNotesColumn &&
            hasCheckinProofImageUriColumn &&
            hasCheckinProofImageDisplayNameColumn &&
            hasCheckinProofImageSizeBytesColumn &&
            hasCheckinProofImageModifiedOnColumn &&
            hasAutomatedBackupSettingsTable &&
            hasAutomatedBackupCloudEnabledColumn &&
            hasAutomatedBackupSettingsRow &&
            hasReminderSettingsTable &&
            hasReminderTimeLocalColumn &&
            hasReminderSettingsRow)
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
            _logger.LogDebug("Added Habits.Description column to {DatabasePath}.", databasePath);
        }

        if (!hasCheckinNotesColumn)
        {
            var copyLegacyCheckinsSql = hasCheckinProofImageUriColumn
                ? """
                  INSERT INTO Checkins (CheckinDate, HabitId, ProofImageUri)
                  SELECT CheckinDate, HabitId, ProofImageUri
                  FROM Checkins_Legacy;
                  """
                : """
                  INSERT INTO Checkins (CheckinDate, HabitId)
                  SELECT CheckinDate, HabitId
                  FROM Checkins_Legacy;
                  """;

            command.CommandText =
                $$"""
                  ALTER TABLE Checkins RENAME TO Checkins_Legacy;

                  CREATE TABLE Checkins (
                      CheckinDate TEXT NOT NULL,
                      HabitId INTEGER NOT NULL,
                      Notes TEXT NULL,
                      ProofImageUri TEXT NULL,
                      ProofImageDisplayName TEXT NULL,
                      ProofImageSizeBytes INTEGER NULL,
                      ProofImageModifiedOn TEXT NULL,
                      CONSTRAINT PK_Checkins PRIMARY KEY (HabitId, CheckinDate),
                      CONSTRAINT FK_Checkins_Habits FOREIGN KEY (HabitId) REFERENCES Habits (Id) ON DELETE CASCADE ON UPDATE CASCADE,
                      CONSTRAINT CK_Checkins_CheckinDate CHECK (
                          length (CheckinDate) = 10
                          AND CheckinDate GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]'
                          AND strftime ('%Y-%m-%d', CheckinDate) IS NOT NULL
                          AND strftime ('%Y-%m-%d', CheckinDate) = CheckinDate
                      ),
                      CONSTRAINT CK_Checkins_Notes_Length CHECK (
                          Notes IS NULL OR length(Notes) <= {{CoreConstants.CheckinNotesMaxLength}}
                      )
                  ) STRICT;

                  {{copyLegacyCheckinsSql}}

                  DROP TABLE Checkins_Legacy;
                  """;
            command.ExecuteNonQuery();
            _logger.LogDebug("Rebuilt Checkins table with nullable Notes column in {DatabasePath}.", databasePath);
        }

        if (!hasCheckinProofImageUriColumn && hasCheckinNotesColumn)
        {
            command.CommandText = "ALTER TABLE Checkins ADD COLUMN ProofImageUri TEXT NULL;";
            command.ExecuteNonQuery();
            _logger.LogDebug("Added Checkins.ProofImageUri column to {DatabasePath}.", databasePath);
        }

        if (!hasCheckinProofImageDisplayNameColumn && hasCheckinNotesColumn)
        {
            command.CommandText = "ALTER TABLE Checkins ADD COLUMN ProofImageDisplayName TEXT NULL;";
            command.ExecuteNonQuery();
            _logger.LogDebug("Added Checkins.ProofImageDisplayName column to {DatabasePath}.", databasePath);
        }

        if (!hasCheckinProofImageSizeBytesColumn && hasCheckinNotesColumn)
        {
            command.CommandText = "ALTER TABLE Checkins ADD COLUMN ProofImageSizeBytes INTEGER NULL;";
            command.ExecuteNonQuery();
            _logger.LogDebug("Added Checkins.ProofImageSizeBytes column to {DatabasePath}.", databasePath);
        }

        if (!hasCheckinProofImageModifiedOnColumn && hasCheckinNotesColumn)
        {
            command.CommandText = "ALTER TABLE Checkins ADD COLUMN ProofImageModifiedOn TEXT NULL;";
            command.ExecuteNonQuery();
            _logger.LogDebug("Added Checkins.ProofImageModifiedOn column to {DatabasePath}.", databasePath);
        }

        if (!hasAutomatedBackupSettingsTable)
        {
            command.CommandText =
                $"""
                 CREATE TABLE IF NOT EXISTS {AutomatedBackupConstants.SettingsTableName} (
                     Id INTEGER NOT NULL,
                     {AutomatedBackupConstants.LocalEnabledColumnName} INTEGER NOT NULL DEFAULT 0,
                     {AutomatedBackupConstants.CloudEnabledColumnName} INTEGER NOT NULL DEFAULT 0,
                     CONSTRAINT PK_{AutomatedBackupConstants.SettingsTableName} PRIMARY KEY (Id),
                     CONSTRAINT CK_{AutomatedBackupConstants.SettingsTableName}_{AutomatedBackupConstants.LocalEnabledColumnName} CHECK ({AutomatedBackupConstants.LocalEnabledColumnName} IN (0, 1)),
                     CONSTRAINT CK_{AutomatedBackupConstants.SettingsTableName}_{AutomatedBackupConstants.CloudEnabledColumnName} CHECK ({AutomatedBackupConstants.CloudEnabledColumnName} IN (0, 1))
                 ) STRICT;
                 """;
            command.ExecuteNonQuery();
        }

        if (hasAutomatedBackupSettingsTable && !hasAutomatedBackupCloudEnabledColumn)
        {
            command.CommandText =
                $"""
                 ALTER TABLE {AutomatedBackupConstants.SettingsTableName}
                 ADD COLUMN {AutomatedBackupConstants.CloudEnabledColumnName} INTEGER NOT NULL DEFAULT 0
                 CHECK ({AutomatedBackupConstants.CloudEnabledColumnName} IN (0, 1));
                 """;
            command.ExecuteNonQuery();
        }

        if (!hasAutomatedBackupSettingsRow)
        {
            command.CommandText =
                $"""
                 INSERT INTO {AutomatedBackupConstants.SettingsTableName} (Id, {AutomatedBackupConstants.LocalEnabledColumnName}, {AutomatedBackupConstants.CloudEnabledColumnName})
                 VALUES ({AutomatedBackupConstants.SettingsRowId}, 0, 0)
                 ON CONFLICT(Id) DO NOTHING;
                 """;
            command.ExecuteNonQuery();
        }

        if (hasAutomatedBackupSettingsTable || !hasAutomatedBackupSettingsRow)
        {
            command.CommandText =
                $"""
                 UPDATE {AutomatedBackupConstants.SettingsTableName}
                 SET {AutomatedBackupConstants.CloudEnabledColumnName} = 0
                 WHERE {AutomatedBackupConstants.CloudEnabledColumnName} IS NULL;
                 """;
            command.ExecuteNonQuery();
        }

        if (!hasReminderSettingsTable)
        {
            command.CommandText =
                $"""
                 CREATE TABLE IF NOT EXISTS {ReminderConstants.SettingsTableName} (
                     Id INTEGER NOT NULL,
                     IsEnabled INTEGER NOT NULL DEFAULT 0,
                     {ReminderConstants.TimeLocalColumnName} TEXT NOT NULL DEFAULT '{CoreConstants.DefaultReminderTimeLocal}',
                     CONSTRAINT PK_{ReminderConstants.SettingsTableName} PRIMARY KEY (Id),
                     CONSTRAINT CK_{ReminderConstants.SettingsTableName}_IsEnabled CHECK (IsEnabled IN (0, 1)),
                     CONSTRAINT CK_{ReminderConstants.SettingsTableName}_{ReminderConstants.TimeLocalColumnName} CHECK (
                         length({ReminderConstants.TimeLocalColumnName}) = 8
                         AND {ReminderConstants.TimeLocalColumnName} GLOB '[0-2][0-9]:[0-5][0-9]:[0-5][0-9]'
                         AND time({ReminderConstants.TimeLocalColumnName}) IS NOT NULL
                         AND time({ReminderConstants.TimeLocalColumnName}) = {ReminderConstants.TimeLocalColumnName}
                     )
                 ) STRICT;
                 """;
            command.ExecuteNonQuery();
        }

        if (hasReminderSettingsTable && !hasReminderTimeLocalColumn)
        {
            command.CommandText =
                $"""
                 ALTER TABLE {ReminderConstants.SettingsTableName}
                 ADD COLUMN {ReminderConstants.TimeLocalColumnName} TEXT NOT NULL DEFAULT '{CoreConstants.DefaultReminderTimeLocal}';
                 """;
            command.ExecuteNonQuery();
        }

        if (!hasReminderSettingsRow)
        {
            command.CommandText =
                $"""
                 INSERT INTO {ReminderConstants.SettingsTableName} (Id, IsEnabled, {ReminderConstants.TimeLocalColumnName})
                 VALUES ({ReminderConstants.SettingsRowId}, 0, '{CoreConstants.DefaultReminderTimeLocal}')
                 ON CONFLICT(Id) DO NOTHING;
                 """;
            command.ExecuteNonQuery();
        }

        if (hasReminderSettingsTable || !hasReminderSettingsRow)
        {
            command.CommandText =
                $"""
                 UPDATE {ReminderConstants.SettingsTableName}
                 SET {ReminderConstants.TimeLocalColumnName} = '{CoreConstants.DefaultReminderTimeLocal}'
                 WHERE {ReminderConstants.TimeLocalColumnName} IS NULL
                    OR trim({ReminderConstants.TimeLocalColumnName}) = '';
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
