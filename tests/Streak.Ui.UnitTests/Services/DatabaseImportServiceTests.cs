namespace Streak.Ui.UnitTests.Services;

public sealed class DatabaseImportServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ImportDatabaseAsync_ShouldUpgradeLegacyBackupBeforeLeavingItLive()
    {
        using var backupDirectory = new TemporaryDirectory();
        using var appDataDirectory = new TemporaryDirectory();
        using var liveProofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var backupDatabasePath = Path.Combine(backupDirectory.Path, "legacy-backup.db");
        var backupArchivePath = Path.Combine(backupDirectory.Path, "legacy-backup.zip");
        var liveDatabasePath = Path.Combine(appDataDirectory.Path, "streak.local.db");

        CreateLegacyDatabase(backupDatabasePath);
        CreateBackupArchive(backupArchivePath, backupDatabasePath);
        CreatePlaceholderDatabase(liveDatabasePath);

        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(liveDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(liveProofDirectory.Path);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectory.Path);

        var schemaUpgrader = new SqliteDatabaseSchemaUpgrader(new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>().Object);
        var sut = new DatabaseImportService(
            appStoragePathServiceMock.Object,
            new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object),
            schemaUpgrader,
            new Mock<ILogger<DatabaseImportService>>().Object);

        await sut.ImportDatabaseAsync(new FileResult(backupArchivePath));

        var habitColumns = GetTableColumns(liveDatabasePath, "Habits");
        habitColumns.Should().Contain("Description");
        var checkinColumns = GetTableColumns(liveDatabasePath, "Checkins");
        checkinColumns.Should().Contain("Notes");
        checkinColumns.Should().Contain("ProofImageUri");
        checkinColumns.Should().Contain("ProofImageDisplayName");
        checkinColumns.Should().Contain("ProofImageSizeBytes");
        checkinColumns.Should().Contain("ProofImageModifiedOn");

        using var connection = OpenConnection(liveDatabasePath);
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
    }

    [Fact]
    public async Task ImportDatabaseAsync_ShouldRestorePictureProofFilesFromBackupArchive()
    {
        using var backupDirectory = new TemporaryDirectory();
        using var backupProofDirectory = new TemporaryDirectory();
        using var appDataDirectory = new TemporaryDirectory();
        using var liveProofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var backupDatabasePath = Path.Combine(backupDirectory.Path, "current-backup.db");
        var backupArchivePath = Path.Combine(backupDirectory.Path, "current-backup.zip");
        var liveDatabasePath = Path.Combine(appDataDirectory.Path, "streak.local.db");
        var proofRelativePath = "Habit-7/2026/04/2026-04-21/proof.jpg";
        var expectedProofBytes = new byte[] { 9, 8, 7, 6 };
        var staleProofPath = Path.Combine(liveProofDirectory.Path, "old-proof.jpg");

        CreateCurrentDatabase(backupDatabasePath, proofRelativePath);
        CreateProofFile(backupProofDirectory.Path, proofRelativePath, expectedProofBytes);
        CreateBackupArchive(backupArchivePath, backupDatabasePath, backupProofDirectory.Path, proofRelativePath);
        CreatePlaceholderDatabase(liveDatabasePath);
        File.WriteAllText(staleProofPath, "stale");

        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(liveDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(liveProofDirectory.Path);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectory.Path);

        var schemaUpgrader = new SqliteDatabaseSchemaUpgrader(new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>().Object);
        var sut = new DatabaseImportService(
            appStoragePathServiceMock.Object,
            new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object),
            schemaUpgrader,
            new Mock<ILogger<DatabaseImportService>>().Object);

        await sut.ImportDatabaseAsync(new FileResult(backupArchivePath));

        File.Exists(staleProofPath).Should().BeFalse();

        var restoredProofPath = Path.Combine(
            [.. new[] { liveProofDirectory.Path }, .. proofRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries)]);
        File.Exists(restoredProofPath).Should().BeTrue();
        File.ReadAllBytes(restoredProofPath).Should().Equal(expectedProofBytes);
    }

    [Fact]
    public async Task ImportDatabaseAsync_ShouldKeepExistingProofFiles_WhenDirectDatabaseBackupIsSelected()
    {
        using var backupDirectory = new TemporaryDirectory();
        using var appDataDirectory = new TemporaryDirectory();
        using var liveProofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var backupDatabasePath = Path.Combine(backupDirectory.Path, "legacy-backup.db");
        var liveDatabasePath = Path.Combine(appDataDirectory.Path, "streak.local.db");
        var existingProofRelativePath = "Habit-2/2026/04/2026-04-21/existing-proof.jpg";
        var existingProofBytes = new byte[] { 5, 4, 3, 2, 1 };

        CreateLegacyDatabase(backupDatabasePath);
        CreatePlaceholderDatabase(liveDatabasePath);
        CreateProofFile(liveProofDirectory.Path, existingProofRelativePath, existingProofBytes);

        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(liveDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(liveProofDirectory.Path);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectory.Path);

        var schemaUpgrader = new SqliteDatabaseSchemaUpgrader(new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>().Object);
        var sut = new DatabaseImportService(
            appStoragePathServiceMock.Object,
            new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object),
            schemaUpgrader,
            new Mock<ILogger<DatabaseImportService>>().Object);

        await sut.ImportDatabaseAsync(new FileResult(backupDatabasePath));

        var habitColumns = GetTableColumns(liveDatabasePath, "Habits");
        habitColumns.Should().Contain("Description");
        var restoredProofPath = Path.Combine(
            [.. new[] { liveProofDirectory.Path }, .. existingProofRelativePath.Split('/', StringSplitOptions.RemoveEmptyEntries)]);
        File.Exists(restoredProofPath).Should().BeTrue();
        File.ReadAllBytes(restoredProofPath).Should().Equal(existingProofBytes);
    }

    [Fact]
    public async Task ImportDatabaseAsync_ShouldClearMissingProofMetadata_WhenDirectDatabaseBackupReferencesUnavailableProofFiles()
    {
        using var backupDirectory = new TemporaryDirectory();
        using var appDataDirectory = new TemporaryDirectory();
        using var liveProofDirectory = new TemporaryDirectory();
        using var exportDirectory = new TemporaryDirectory();

        var backupDatabasePath = Path.Combine(backupDirectory.Path, "current-backup.db");
        var liveDatabasePath = Path.Combine(appDataDirectory.Path, "streak.local.db");
        var missingProofRelativePath = "Habit-7/2026/04/2026-04-21/missing-proof.jpg";

        CreateCurrentDatabase(backupDatabasePath, missingProofRelativePath);
        CreatePlaceholderDatabase(liveDatabasePath);

        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(liveDatabasePath);
        appStoragePathServiceMock.SetupGet(x => x.CheckinProofsDirectoryPath).Returns(liveProofDirectory.Path);
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectory.Path);

        var schemaUpgrader = new SqliteDatabaseSchemaUpgrader(new Mock<ILogger<SqliteDatabaseSchemaUpgrader>>().Object);
        var sut = new DatabaseImportService(
            appStoragePathServiceMock.Object,
            new FileSystemCheckinProofFileStore(appStoragePathServiceMock.Object),
            schemaUpgrader,
            new Mock<ILogger<DatabaseImportService>>().Object);

        await sut.ImportDatabaseAsync(new FileResult(backupDatabasePath));

        using var connection = OpenConnection(liveDatabasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
            SELECT HabitId, CheckinDate, ProofImageUri, ProofImageDisplayName, ProofImageSizeBytes, ProofImageModifiedOn
            FROM Checkins;
            """;

        using var reader = command.ExecuteReader();
        reader.Read().Should().BeTrue();
        reader.GetInt64(0).Should().Be(7);
        reader.GetString(1).Should().Be("2026-04-21");
        reader.IsDBNull(2).Should().BeTrue();
        reader.IsDBNull(3).Should().BeTrue();
        reader.IsDBNull(4).Should().BeTrue();
        reader.IsDBNull(5).Should().BeTrue();
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

            INSERT INTO Checkins (CheckinDate, HabitId)
            VALUES ('2025-01-01', 1);
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

    private static void CreateCurrentDatabase(string databasePath, string proofImageUri)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            """
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
                ProofImageUri TEXT NULL,
                ProofImageDisplayName TEXT NULL,
                ProofImageSizeBytes INTEGER NULL,
                ProofImageModifiedOn TEXT NULL,
                PRIMARY KEY (HabitId, CheckinDate)
            );

            INSERT INTO Habits (Id, Name, Emoji, Description)
            VALUES (7, 'Practice', '🎯', NULL);

            INSERT INTO Checkins (CheckinDate, HabitId, Notes, ProofImageUri, ProofImageDisplayName, ProofImageSizeBytes, ProofImageModifiedOn)
            VALUES ('2026-04-21', 7, NULL, $proofImageUri, 'proof.jpg', 4, '2026-04-21T08:30:12.0000000+00:00');
            """;
        command.Parameters.AddWithValue("$proofImageUri", proofImageUri);
        command.ExecuteNonQuery();
    }

    private static void CreateBackupArchive(
        string archivePath,
        string databasePath,
        string? proofDirectoryPath = null,
        string? relativeProofPath = null)
    {
        using var archiveStream = File.Create(archivePath);
        using var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create);

        AddArchiveEntryFromFile(archive, databasePath, "streak.db");

        if (!string.IsNullOrWhiteSpace(proofDirectoryPath) && !string.IsNullOrWhiteSpace(relativeProofPath))
        {
            var proofPath = Path.Combine(
                [.. new[] { proofDirectoryPath }, .. relativeProofPath.Split('/', StringSplitOptions.RemoveEmptyEntries)]);
            AddArchiveEntryFromFile(
                archive,
                proofPath,
                $"CheckinProofs/{relativeProofPath}");
        }
    }

    private static void AddArchiveEntryFromFile(ZipArchive archive, string sourceFilePath, string entryPath)
    {
        var entry = archive.CreateEntry(entryPath, CompressionLevel.Optimal);

        using var sourceStream = File.OpenRead(sourceFilePath);
        using var entryStream = entry.Open();
        sourceStream.CopyTo(entryStream);
    }

    private static void CreateProofFile(string proofDirectoryPath, string relativeProofPath, byte[] proofBytes)
    {
        var proofPath = Path.Combine(
            [.. new[] { proofDirectoryPath }, .. relativeProofPath.Split('/', StringSplitOptions.RemoveEmptyEntries)]);
        Directory.CreateDirectory(Path.GetDirectoryName(proofPath)!);
        File.WriteAllBytes(proofPath, proofBytes);
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
