namespace Streak.Ui.UnitTests.Services;

public sealed class ReminderConfigurationServiceTests
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
    public void GetIsEnabled_ShouldReturnPersistedValue()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, timeLocal: new TimeOnly(21, 0));

        var schedulerMock = new Mock<IReminderScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.GetIsEnabled().Should().BeFalse();
    }

    [Fact]
    public void GetIsEnabled_ShouldReturnFalse_WhenDatabaseDoesNotExist()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "missing.db");

        var schedulerMock = new Mock<IReminderScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.GetIsEnabled().Should().BeFalse();
    }

    [Fact]
    public void GetTimeLocal_ShouldReturnPersistedValue()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: true, timeLocal: new TimeOnly(20, 15));

        var schedulerMock = new Mock<IReminderScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.GetTimeLocal().Should().Be(new TimeOnly(20, 15));
    }

    [Fact]
    public void SetIsEnabled_ShouldPersistEnabledValueAndSynchronizeScheduler()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, timeLocal: new TimeOnly(21, 0));

        var schedulerMock = new Mock<IReminderScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.SetIsEnabled(true);

        ReminderSettingsStore.GetSettings(databasePath).Should().Be(new ReminderSettings(true, new TimeOnly(21, 0)));
        schedulerMock.Verify(x => x.Synchronize(true, new TimeOnly(21, 0)), Times.Once);
    }

    [Fact]
    public void SetTimeLocal_ShouldPersistTimeAndSynchronizeScheduler()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: true, timeLocal: new TimeOnly(21, 0));

        var schedulerMock = new Mock<IReminderScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.SetTimeLocal(new TimeOnly(19, 45));

        ReminderSettingsStore.GetSettings(databasePath).Should().Be(new ReminderSettings(true, new TimeOnly(19, 45)));
        schedulerMock.Verify(x => x.Synchronize(true, new TimeOnly(19, 45)), Times.Once);
    }

    [Fact]
    public void SynchronizeScheduler_ShouldApplyPersistedSettings()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, timeLocal: new TimeOnly(18, 30));

        var schedulerMock = new Mock<IReminderScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.SynchronizeScheduler();

        schedulerMock.Verify(x => x.Synchronize(false, new TimeOnly(18, 30)), Times.Once);
    }

    #endregion

    #region Negative tests

    [Fact]
    public void SetTimeLocal_ShouldRestorePreviousValue_WhenSchedulerSynchronizationFails()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: true, timeLocal: new TimeOnly(21, 0));

        var schedulerMock = new Mock<IReminderScheduler>();
        schedulerMock
            .SetupSequence(x => x.Synchronize(It.IsAny<bool>(), It.IsAny<TimeOnly>()))
            .Throws(new InvalidOperationException("Scheduling failed."))
            .Pass();

        var sut = CreateSut(databasePath, schedulerMock.Object);

        var act = () => sut.SetTimeLocal(new TimeOnly(19, 0));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Scheduling failed.");
        ReminderSettingsStore.GetSettings(databasePath).Should().Be(new ReminderSettings(true, new TimeOnly(21, 0)));
        schedulerMock.Verify(x => x.Synchronize(true, new TimeOnly(19, 0)), Times.Once);
        schedulerMock.Verify(x => x.Synchronize(true, new TimeOnly(21, 0)), Times.Once);
    }

    #endregion

    #region Private Helper Methods

    private static ReminderConfigurationService CreateSut(
        string databasePath,
        IReminderScheduler reminderScheduler)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(databasePath);
        var loggerMock = new Mock<ILogger<ReminderConfigurationService>>();

        return new ReminderConfigurationService(
            appStoragePathServiceMock.Object,
            reminderScheduler,
            loggerMock.Object);
    }

    private static void CreateDatabase(string databasePath, bool isEnabled, TimeOnly timeLocal)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TABLE {ReminderConstants.SettingsTableName} (
                 Id INTEGER NOT NULL PRIMARY KEY,
                 IsEnabled INTEGER NOT NULL DEFAULT 0,
                 {ReminderConstants.TimeLocalColumnName} TEXT NOT NULL
             ) STRICT;

             INSERT INTO {ReminderConstants.SettingsTableName} (Id, IsEnabled, {ReminderConstants.TimeLocalColumnName})
             VALUES ({ReminderConstants.SettingsRowId}, {(isEnabled ? 1 : 0)}, '{timeLocal.ToString(ReminderConstants.TimeStorageFormat, CultureInfo.InvariantCulture)}');
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

    #endregion
}
