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
        var checkinColumns = GetTableColumns(databasePath, "Checkins");
        checkinColumns.Should().Contain("Notes");
        checkinColumns.Should().Contain("ProofImageUri");
        checkinColumns.Should().Contain("ProofImageDisplayName");
        checkinColumns.Should().Contain("ProofImageSizeBytes");
        checkinColumns.Should().Contain("ProofImageModifiedOn");

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, Emoji, Description FROM Habits WHERE Id = 1;";

        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetString(0).Should().Be("Read");
        reader.GetString(1).Should().Be("📖");
        reader.IsDBNull(2).Should().BeTrue();

        using var checkinCommand = connection.CreateCommand();
        checkinCommand.CommandText = "SELECT HabitId, CheckinDate, Notes, ProofImageUri, ProofImageDisplayName, ProofImageSizeBytes, ProofImageModifiedOn FROM Checkins;";
        using var checkinReader = checkinCommand.ExecuteReader();
        checkinReader.Read().Should().BeTrue();
        checkinReader.GetInt64(0).Should().Be(1);
        checkinReader.GetString(1).Should().Be("2025-01-01");
        checkinReader.IsDBNull(2).Should().BeTrue();
        checkinReader.IsDBNull(3).Should().BeTrue();
        checkinReader.IsDBNull(4).Should().BeTrue();
        checkinReader.IsDBNull(5).Should().BeTrue();
        checkinReader.IsDBNull(6).Should().BeTrue();

        GetAutomatedBackupSetting(databasePath).Should().BeFalse();
        GetReminderSettings(databasePath).Should().Be(new ReminderSettings(false, new TimeOnly(21, 0)));
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
        var notesColumns = GetTableColumns(databasePath, "Checkins")
            .Where(columnName => string.Equals(columnName, "Notes", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var proofImageUriColumns = GetTableColumns(databasePath, "Checkins")
            .Where(columnName => string.Equals(columnName, "ProofImageUri", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var proofImageDisplayNameColumns = GetTableColumns(databasePath, "Checkins")
            .Where(columnName => string.Equals(columnName, "ProofImageDisplayName", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var proofImageSizeBytesColumns = GetTableColumns(databasePath, "Checkins")
            .Where(columnName => string.Equals(columnName, "ProofImageSizeBytes", StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var proofImageModifiedOnColumns = GetTableColumns(databasePath, "Checkins")
            .Where(columnName => string.Equals(columnName, "ProofImageModifiedOn", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        descriptionColumns.Should().ContainSingle();
        notesColumns.Should().ContainSingle();
        proofImageUriColumns.Should().ContainSingle();
        proofImageDisplayNameColumns.Should().ContainSingle();
        proofImageSizeBytesColumns.Should().ContainSingle();
        proofImageModifiedOnColumns.Should().ContainSingle();
        GetTableColumns(databasePath, AutomatedBackupConstants.SettingsTableName)
            .Should()
            .Contain(["Id", "IsEnabled"]);
        GetTableColumns(databasePath, ReminderConstants.SettingsTableName)
            .Should()
            .Contain(["Id", "IsEnabled", "TimeLocal"]);
        GetAutomatedBackupSetting(databasePath).Should().BeFalse();
        GetReminderSettings(databasePath).Should().Be(new ReminderSettings(false, new TimeOnly(21, 0)));
    }

    [Fact]
    public void UpgradeIfNeeded_ShouldAddProofMetadataColumns_WhenCheckinsAlreadyHaveNotes()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "partial.db");
        CreateDatabaseWithoutProofMetadataColumns(databasePath);

        var loggerMock = new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>();
        var sut = new SqliteDatabaseSchemaUpgrader(loggerMock.Object);

        sut.UpgradeIfNeeded(databasePath);

        var checkinColumns = GetTableColumns(databasePath, "Checkins");
        checkinColumns.Should().Contain("ProofImageUri");
        checkinColumns.Should().Contain("ProofImageDisplayName");
        checkinColumns.Should().Contain("ProofImageSizeBytes");
        checkinColumns.Should().Contain("ProofImageModifiedOn");

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT HabitId, CheckinDate, Notes, ProofImageUri, ProofImageDisplayName, ProofImageSizeBytes, ProofImageModifiedOn FROM Checkins;";

        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetInt64(0).Should().Be(1);
        reader.GetString(1).Should().Be("2025-01-01");
        reader.GetString(2).Should().Be("Existing note");
        reader.IsDBNull(3).Should().BeTrue();
        reader.IsDBNull(4).Should().BeTrue();
        reader.IsDBNull(5).Should().BeTrue();
        reader.IsDBNull(6).Should().BeTrue();
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

            CREATE TABLE Checkins (
                CheckinDate TEXT NOT NULL,
                HabitId INTEGER NOT NULL,
                PRIMARY KEY (HabitId, CheckinDate)
            );

            INSERT INTO Habits (Id, Name, Emoji)
            VALUES (1, 'Read', '📖');

            INSERT INTO Checkins (CheckinDate, HabitId)
            VALUES ('2025-01-01', 1);
            """;
        command.ExecuteNonQuery();
    }

    private static void CreateDatabaseWithoutProofMetadataColumns(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE Habits (
                Id INTEGER NOT NULL PRIMARY KEY,
                Name TEXT NOT NULL,
                Emoji TEXT NULL,
                Description TEXT NULL
            );

            CREATE TABLE Checkins (
                CheckinDate TEXT NOT NULL,
                HabitId INTEGER NOT NULL,
                Notes TEXT NULL,
                PRIMARY KEY (HabitId, CheckinDate)
            );

            CREATE TABLE {AutomatedBackupConstants.SettingsTableName} (
                Id INTEGER NOT NULL PRIMARY KEY,
                IsEnabled INTEGER NOT NULL DEFAULT 0
            );

            INSERT INTO {AutomatedBackupConstants.SettingsTableName} (Id, IsEnabled)
            VALUES ({AutomatedBackupConstants.SettingsRowId}, 0);

            INSERT INTO Habits (Id, Name, Emoji, Description)
            VALUES (1, 'Read', '📖', 'Daily reading');

            INSERT INTO Checkins (CheckinDate, HabitId, Notes)
            VALUES ('2025-01-01', 1, 'Existing note');
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

    private static ReminderSettings GetReminderSettings(string databasePath)
    {
        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             SELECT IsEnabled, {ReminderConstants.TimeLocalColumnName}
             FROM {ReminderConstants.SettingsTableName}
             WHERE Id = {ReminderConstants.SettingsRowId};
             """;

        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();

        return new ReminderSettings(
            reader.GetInt64(0) == 1,
            TimeOnly.ParseExact(reader.GetString(1), ReminderConstants.TimeStorageFormat, CultureInfo.InvariantCulture));
    }

    #endregion
}
