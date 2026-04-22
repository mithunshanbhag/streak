namespace Streak.Ui.Misc.Utilities;

public static class ReminderSettingsStore
{
    public static ReminderSettings GetSettings(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!File.Exists(databasePath))
            return GetDefaultSettings();

        using var connection = OpenConnection(databasePath, SqliteOpenMode.ReadOnly);
        if (!TableExists(connection))
            return GetDefaultSettings();

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             SELECT IsEnabled, {ReminderConstants.TimeLocalColumnName}
             FROM {ReminderConstants.SettingsTableName}
             WHERE Id = $id
             LIMIT 1;
             """;
        command.Parameters.AddWithValue("$id", ReminderConstants.SettingsRowId);

        using var reader = command.ExecuteReader();
        if (!reader.Read())
            return GetDefaultSettings();

        var isEnabled = !reader.IsDBNull(0) && reader.GetInt64(0) == 1;
        var timeLocal = reader.IsDBNull(1)
            ? GetDefaultTimeLocal()
            : ParseTimeLocal(reader.GetString(1));

        return new ReminderSettings(isEnabled, timeLocal);
    }

    public static void SetSettings(string databasePath, ReminderSettings settings)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!File.Exists(databasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", databasePath);

        using var connection = OpenConnection(databasePath, SqliteOpenMode.ReadWrite);
        if (!TableExists(connection))
            throw new InvalidOperationException("The reminder settings table is missing from the local database.");

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             INSERT INTO {ReminderConstants.SettingsTableName} (Id, IsEnabled, {ReminderConstants.TimeLocalColumnName})
             VALUES ($id, $isEnabled, $timeLocal)
             ON CONFLICT(Id) DO UPDATE SET
                 IsEnabled = excluded.IsEnabled,
                 {ReminderConstants.TimeLocalColumnName} = excluded.{ReminderConstants.TimeLocalColumnName};
             """;
        command.Parameters.AddWithValue("$id", ReminderConstants.SettingsRowId);
        command.Parameters.AddWithValue("$isEnabled", settings.IsEnabled ? 1 : 0);
        command.Parameters.AddWithValue("$timeLocal", FormatTimeLocal(settings.TimeLocal));
        command.ExecuteNonQuery();
    }

    #region Private Helper Methods

    private static ReminderSettings GetDefaultSettings()
    {
        return new ReminderSettings(false, GetDefaultTimeLocal());
    }

    private static TimeOnly GetDefaultTimeLocal()
    {
        return ParseTimeLocal(CoreConstants.DefaultReminderTimeLocal);
    }

    private static string FormatTimeLocal(TimeOnly timeLocal)
    {
        return timeLocal.ToString(ReminderConstants.TimeStorageFormat, CultureInfo.InvariantCulture);
    }

    private static TimeOnly ParseTimeLocal(string value)
    {
        return TimeOnly.ParseExact(value, ReminderConstants.TimeStorageFormat, CultureInfo.InvariantCulture);
    }

    private static SqliteConnection OpenConnection(string databasePath, SqliteOpenMode mode)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = mode,
            Pooling = false
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static bool TableExists(SqliteConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type = 'table'
              AND name = $tableName;
            """;
        command.Parameters.AddWithValue("$tableName", ReminderConstants.SettingsTableName);

        var result = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        return result > 0;
    }

    #endregion
}
