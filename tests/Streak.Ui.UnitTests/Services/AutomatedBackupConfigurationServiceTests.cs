namespace Streak.Ui.UnitTests.Services;

public sealed class AutomatedBackupConfigurationServiceTests
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
        CreateDatabase(databasePath, isEnabled: true);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.GetIsEnabled().Should().BeTrue();
    }

    [Fact]
    public void GetIsCloudEnabled_ShouldReturnPersistedValue()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, isCloudEnabled: true);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.GetIsCloudEnabled().Should().BeTrue();
    }

    [Fact]
    public void GetHasAnyEnabled_ShouldReturnTrue_WhenCloudBackupIsEnabled()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, isCloudEnabled: true);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.GetHasAnyEnabled().Should().BeTrue();
    }

    [Fact]
    public void IsSupported_ShouldMirrorSchedulerSupport()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        schedulerMock.SetupGet(x => x.IsSupported).Returns(true);
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.IsSupported.Should().BeTrue();
    }

    [Fact]
    public void SetIsEnabled_ShouldPersistEnabledValueAndSynchronizeScheduler()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.SetIsEnabled(true);

        AutomatedBackupSettingsStore.GetIsEnabled(databasePath).Should().BeTrue();
        schedulerMock.Verify(x => x.Synchronize(true), Times.Once);
    }

    [Fact]
    public void SetIsCloudEnabled_ShouldPersistEnabledValueAndSynchronizeScheduler()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, isCloudEnabled: false);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.SetIsCloudEnabled(true);

        AutomatedBackupSettingsStore.GetIsCloudEnabled(databasePath).Should().BeTrue();
        schedulerMock.Verify(x => x.Synchronize(true), Times.Once);
    }

    [Fact]
    public void SynchronizeScheduler_ShouldApplyCombinedPersistedValue()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, isCloudEnabled: true);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        var sut = CreateSut(databasePath, schedulerMock.Object);

        sut.SynchronizeScheduler();

        schedulerMock.Verify(x => x.Synchronize(true), Times.Once);
    }

    #endregion

    #region Negative tests

    [Fact]
    public void SetIsEnabled_ShouldRestorePreviousValue_WhenSchedulerSynchronizationFails()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        schedulerMock
            .SetupSequence(x => x.Synchronize(It.IsAny<bool>()))
            .Throws(new InvalidOperationException("Scheduling failed."))
            .Pass();

        var sut = CreateSut(databasePath, schedulerMock.Object);

        var act = () => sut.SetIsEnabled(true);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Scheduling failed.");
        AutomatedBackupSettingsStore.GetIsEnabled(databasePath).Should().BeFalse();
        schedulerMock.Verify(x => x.Synchronize(true), Times.Once);
        schedulerMock.Verify(x => x.Synchronize(false), Times.Once);
    }

    [Fact]
    public void SetIsCloudEnabled_ShouldRestorePreviousValue_WhenSchedulerSynchronizationFails()
    {
        using var databaseDirectory = new TemporaryDirectory();
        var databasePath = Path.Combine(databaseDirectory.Path, "settings.db");
        CreateDatabase(databasePath, isEnabled: false, isCloudEnabled: false);

        var schedulerMock = new Mock<IAutomatedBackupScheduler>();
        schedulerMock
            .SetupSequence(x => x.Synchronize(It.IsAny<bool>()))
            .Throws(new InvalidOperationException("Scheduling failed."))
            .Pass();

        var sut = CreateSut(databasePath, schedulerMock.Object);

        var act = () => sut.SetIsCloudEnabled(true);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Scheduling failed.");
        AutomatedBackupSettingsStore.GetIsCloudEnabled(databasePath).Should().BeFalse();
        schedulerMock.Verify(x => x.Synchronize(true), Times.Once);
        schedulerMock.Verify(x => x.Synchronize(false), Times.Once);
    }

    #endregion

    #region Private Helper Methods

    private static AutomatedBackupConfigurationService CreateSut(
        string databasePath,
        IAutomatedBackupScheduler automatedBackupScheduler)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.DatabasePath).Returns(databasePath);

        return new AutomatedBackupConfigurationService(
            appStoragePathServiceMock.Object,
            automatedBackupScheduler);
    }

    private static void CreateDatabase(string databasePath, bool isEnabled, bool isCloudEnabled = false)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

        using var connection = OpenConnection(databasePath);
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             CREATE TABLE {AutomatedBackupConstants.SettingsTableName} (
                 Id INTEGER NOT NULL PRIMARY KEY,
                 {AutomatedBackupConstants.LocalEnabledColumnName} INTEGER NOT NULL DEFAULT 0,
                 {AutomatedBackupConstants.CloudEnabledColumnName} INTEGER NOT NULL DEFAULT 0
             ) STRICT;

             INSERT INTO {AutomatedBackupConstants.SettingsTableName} (Id, {AutomatedBackupConstants.LocalEnabledColumnName}, {AutomatedBackupConstants.CloudEnabledColumnName})
             VALUES ({AutomatedBackupConstants.SettingsRowId}, {(isEnabled ? 1 : 0)}, {(isCloudEnabled ? 1 : 0)});
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
