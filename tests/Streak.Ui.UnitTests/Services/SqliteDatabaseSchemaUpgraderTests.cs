namespace Streak.Ui.UnitTests.Services;

public sealed class SqliteDatabaseSchemaUpgraderTests
{
    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"streak-ui-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
    }

    #region Positive tests

    [Fact]
    public void UpgradeIfNeeded_ShouldAddDescriptionColumnAndPreserveExistingHabitData()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "legacy.db");
        CreateLegacyDatabase(databasePath);

        var loggerMock = new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>();
        var sut = new SqliteDatabaseSchemaUpgrader(loggerMock.Object);

        sut.UpgradeIfNeeded(databasePath);

        var habitColumns = GetTableColumns(databasePath, "Habits");
        habitColumns.Should().Contain("Description");

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, Emoji, Description FROM Habits WHERE Id = 1;";

        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetString(0).Should().Be("Read");
        reader.GetString(1).Should().Be("📖");
        reader.IsDBNull(2).Should().BeTrue();

        GetAutomatedBackupSetting(databasePath).Should().BeFalse();
    }

    [Fact]
    public void UpgradeIfNeeded_ShouldBeIdempotent_WhenDescriptionColumnAlreadyExists()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "legacy.db");
        CreateLegacyDatabase(databasePath);

        var loggerMock = new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>();
        var sut = new SqliteDatabaseSchemaUpgrader(loggerMock.Object);

        sut.UpgradeIfNeeded(databasePath);
        sut.UpgradeIfNeeded(databasePath);

        var descriptionColumns = GetTableColumns(databasePath, "Habits")
            .Where(columnName => string.Equals(columnName, "Description", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        descriptionColumns.Should().ContainSingle();
        GetTableColumns(databasePath, AutomatedBackupConstants.SettingsTableName)
            .Should()
            .Contain(["Id", "IsEnabled"]);
        GetAutomatedBackupSetting(databasePath).Should().BeFalse();
    }

    #endregion

    #region Private Helper Methods

    private static void CreateLegacyDatabase(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE Habits (
                Id INTEGER NOT NULL PRIMARY KEY,
                Name TEXT NOT NULL,
                Emoji TEXT NULL
            );

            INSERT INTO Habits (Id, Name, Emoji)
            VALUES (1, 'Read', '📖');
            """;
        command.ExecuteNonQuery();
    }

    private static SqliteConnection OpenConnection(string databasePath)
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Pooling = false
        }.ToString();

        var connection = new SqliteConnection(connectionString);
        connection.Open();
        return connection;
    }

    private static IReadOnlyList<string> GetTableColumns(string databasePath, string tableName)
    {
        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{tableName}\");";

        using var reader = command.ExecuteReader();
        var columnNames = new List<string>();
        while (reader.Read()) columnNames.Add(reader.GetString(1));

        return columnNames;
    }

    private static bool GetAutomatedBackupSetting(string databasePath)
    {
        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             SELECT IsEnabled
             FROM {AutomatedBackupConstants.SettingsTableName}
             WHERE Id = {AutomatedBackupConstants.SettingsRowId};
             """;

        var result = command.ExecuteScalar();
        return Convert.ToInt32(result, CultureInfo.InvariantCulture) == 1;
    }

    #endregion
}
