namespace Streak.Ui.Misc.Utilities;

public static class AutomatedBackupSettingsStore
{
    public static bool GetIsEnabled(string databasePath)
    {
        return GetFlagValue(databasePath, AutomatedBackupConstants.LocalEnabledColumnName);
    }

    public static bool GetIsCloudEnabled(string databasePath)
    {
        return GetFlagValue(databasePath, AutomatedBackupConstants.CloudEnabledColumnName);
    }

    public static bool GetHasAnyEnabled(string databasePath)
    {
        return GetIsEnabled(databasePath) || GetIsCloudEnabled(databasePath);
    }

    public static void SetIsEnabled(string databasePath, bool isEnabled)
    {
        SetFlagValue(databasePath, AutomatedBackupConstants.LocalEnabledColumnName, isEnabled);
    }

    public static void SetIsCloudEnabled(string databasePath, bool isEnabled)
    {
        SetFlagValue(databasePath, AutomatedBackupConstants.CloudEnabledColumnName, isEnabled);
    }

    #region Private Helper Methods

    private static bool GetFlagValue(string databasePath, string columnName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!File.Exists(databasePath))
            return false;

        using var connection = OpenConnection(databasePath, SqliteOpenMode.ReadOnly);
        if (!TableExists(connection))
            return false;

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             SELECT {columnName}
             FROM {AutomatedBackupConstants.SettingsTableName}
             WHERE Id = $id
             LIMIT 1;
             """;
        command.Parameters.AddWithValue("$id", AutomatedBackupConstants.SettingsRowId);

        var result = command.ExecuteScalar();
        if (result is null || result is DBNull)
            return false;

        return Convert.ToInt32(result, CultureInfo.InvariantCulture) == 1;
    }

    private static void SetFlagValue(string databasePath, string columnName, bool isEnabled)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);

        if (!File.Exists(databasePath))
            throw new FileNotFoundException("The local Streak database could not be found.", databasePath);

        using var connection = OpenConnection(databasePath, SqliteOpenMode.ReadWrite);
        if (!TableExists(connection))
            throw new InvalidOperationException("The automated backup settings table is missing from the local database.");

        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             INSERT INTO {AutomatedBackupConstants.SettingsTableName} (Id, {columnName})
             VALUES ($id, $isEnabled)
             ON CONFLICT(Id) DO UPDATE SET {columnName} = excluded.{columnName};
             """;
        command.Parameters.AddWithValue("$id", AutomatedBackupConstants.SettingsRowId);
        command.Parameters.AddWithValue("$isEnabled", isEnabled ? 1 : 0);
        command.ExecuteNonQuery();
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
        command.Parameters.AddWithValue("$tableName", AutomatedBackupConstants.SettingsTableName);

        var result = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
        return result > 0;
    }

    #endregion
}
