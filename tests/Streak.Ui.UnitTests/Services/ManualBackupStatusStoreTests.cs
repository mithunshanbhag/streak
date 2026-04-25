namespace Streak.Ui.UnitTests.Services;

public sealed class ManualBackupStatusStoreTests
{
    #region Positive tests

    [Fact]
    public void ManualBackupStatusStore_ShouldPersistLocalManualBackupTimestamp()
    {
        var store = CreateSut(out var preferenceValues);
        var expectedTimestamp = new DateTimeOffset(2026, 04, 26, 01, 02, 03, TimeSpan.Zero);

        store.SetLastSuccessfulBackupUtc(ManualBackupLocation.Local, expectedTimestamp);

        store.GetLastSuccessfulBackupUtc(ManualBackupLocation.Local).Should().Be(expectedTimestamp);
        preferenceValues["manual-backup.local.last-success-utc"].Should().Be(expectedTimestamp.ToString("O", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ManualBackupStatusStore_ShouldPersistCloudManualBackupTimestamp()
    {
        var store = CreateSut(out var preferenceValues);
        var expectedTimestamp = new DateTimeOffset(2026, 04, 26, 04, 05, 06, TimeSpan.Zero);

        store.SetLastSuccessfulBackupUtc(ManualBackupLocation.Cloud, expectedTimestamp);

        store.GetLastSuccessfulBackupUtc(ManualBackupLocation.Cloud).Should().Be(expectedTimestamp);
        preferenceValues["manual-backup.cloud.last-success-utc"].Should().Be(expectedTimestamp.ToString("O", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void ManualBackupStatusStore_ShouldClearStoredTimestamp()
    {
        var store = CreateSut(out var preferenceValues);
        preferenceValues["manual-backup.cloud.last-success-utc"] = "2026-04-26T04:05:06.0000000+00:00";

        store.Clear(ManualBackupLocation.Cloud);

        store.GetLastSuccessfulBackupUtc(ManualBackupLocation.Cloud).Should().BeNull();
        preferenceValues.ContainsKey("manual-backup.cloud.last-success-utc").Should().BeFalse();
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void ManualBackupStatusStore_ShouldReturnNull_WhenNoTimestampHasBeenPersisted()
    {
        var store = CreateSut(out _);

        store.GetLastSuccessfulBackupUtc(ManualBackupLocation.Local).Should().BeNull();
        store.GetLastSuccessfulBackupUtc(ManualBackupLocation.Cloud).Should().BeNull();
    }

    [Fact]
    public void ManualBackupStatusStore_ShouldClearInvalidPersistedTimestamp()
    {
        var store = CreateSut(out var preferenceValues);
        preferenceValues["manual-backup.local.last-success-utc"] = "not-a-date";

        store.GetLastSuccessfulBackupUtc(ManualBackupLocation.Local).Should().BeNull();
        preferenceValues.ContainsKey("manual-backup.local.last-success-utc").Should().BeFalse();
    }

    #endregion

    #region Private Helper Methods

    private static ManualBackupStatusStore CreateSut(out Dictionary<string, string?> preferenceValues)
    {
        var storedPreferenceValues = new Dictionary<string, string?>();
        preferenceValues = storedPreferenceValues;

        var preferencesMock = new Mock<IPreferences>();
        preferencesMock
            .Setup(x => x.Get<string?>(It.IsAny<string>(), null, null))
            .Returns<string, string?, string?>((key, _, _) =>
                storedPreferenceValues.TryGetValue(key, out var storedValue)
                    ? storedValue
                    : null);
        preferencesMock
            .Setup(x => x.Set(It.IsAny<string>(), It.IsAny<string>(), null))
            .Callback<string, string, string?>((key, value, _) => storedPreferenceValues[key] = value);
        preferencesMock
            .Setup(x => x.Remove(It.IsAny<string>(), null))
            .Callback<string, string?>((key, _) => storedPreferenceValues.Remove(key));

        return new ManualBackupStatusStore(preferencesMock.Object);
    }

    #endregion
}
