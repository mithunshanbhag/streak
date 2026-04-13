namespace Streak.Ui.UnitTests.Services;

public sealed class DatabaseImportServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ImportDatabaseAsync_ShouldUpgradeLegacyBackupBeforeLeavingItLive()
    {
        using var backupDirectory = new TemporaryDirectory();
        using var appDataDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var backupDatabasePath = Path.Combine(backupDirectory.Path, "legacy-backup.db");
        var liveDatabasePath = Path.Combine(appDataDirectory.Path, "streak.local.db");

        CreateLegacyDatabase(backupDatabasePath);
        CreatePlaceholderDatabase(liveDatabasePath);

        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(liveDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectory.Path);

        var schemaUpgrader = new SqliteDatabaseSchemaUpgrader(new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>().Object);
        var sut = new DatabaseImportService(
            appStoragePathServiceMock.Object,
            schemaUpgrader,
            new Mock<ILogger<DatabaseImportService>>().Object);

        await sut.ImportDatabaseAsync(new FileResult(backupDatabasePath));

        var habitColumns = GetTableColumns(liveDatabasePath, "Habits");
        habitColumns.Should().Contain("Description");

        using var connection = OpenConnection(liveDatabasePath);
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT Name, Emoji, Description FROM Habits WHERE Id = 1;";

        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetString(0).Should().Be("Read");
        reader.GetString(1).Should().Be("📖");
        reader.IsDBNull(2).Should().BeTrue();
    }

    #endregion

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
            """;
        command.ExecuteNonQuery();
    }

    private static void CreatePlaceholderDatabase(string databasePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            CREATE TABLE Placeholder (
                Id INTEGER PRIMARY KEY
            );

            INSERT INTO Placeholder (Id)
            VALUES (1);
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

    #endregion
}