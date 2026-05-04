namespace Streak.Ui.UnitTests.Services;

public sealed class OneDriveAuthStateStoreTests
{
    #region Positive tests

    [Fact]
    public void OneDriveAuthStateStore_ShouldPersistLastKnownAccountUsername()
    {
        string? storedAccountUsername = null;
        var preferencesMock = CreatePreferencesMock(
            getLastKnownAccountUsername: () => storedAccountUsername,
            setLastKnownAccountUsername: value => storedAccountUsername = value,
            clearLastKnownAccountUsername: () => storedAccountUsername = null);
        var loggerMock = new Mock<ILogger<OneDriveAuthStateStore>>();
        var store = new OneDriveAuthStateStore(preferencesMock.Object, loggerMock.Object);

        store.SetLastKnownAccountUsername("streak-demo@outlook.com");

        store.GetLastKnownAccountUsername().Should().Be("streak-demo@outlook.com");
    }

    [Fact]
    public void OneDriveAuthStateStore_ShouldClearPersistedAccountUsername()
    {
        string? storedAccountUsername = "streak-demo@outlook.com";
        var preferencesMock = CreatePreferencesMock(
            getLastKnownAccountUsername: () => storedAccountUsername,
            setLastKnownAccountUsername: value => storedAccountUsername = value,
            clearLastKnownAccountUsername: () => storedAccountUsername = null);
        var loggerMock = new Mock<ILogger<OneDriveAuthStateStore>>();
        var store = new OneDriveAuthStateStore(preferencesMock.Object, loggerMock.Object);

        store.Clear();

        store.GetLastKnownAccountUsername().Should().BeNull();
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void OneDriveAuthStateStore_ShouldReturnNull_WhenNoAccountUsernameHasBeenPersisted()
    {
        var preferencesMock = CreatePreferencesMock(
            getLastKnownAccountUsername: () => null,
            setLastKnownAccountUsername: _ => { },
            clearLastKnownAccountUsername: () => { });
        var loggerMock = new Mock<ILogger<OneDriveAuthStateStore>>();
        var store = new OneDriveAuthStateStore(preferencesMock.Object, loggerMock.Object);

        store.GetLastKnownAccountUsername().Should().BeNull();
    }

    [Fact]
    public void OneDriveAuthStateStore_ShouldTreatWhitespaceAccountUsernameAsClear()
    {
        string? storedAccountUsername = "streak-demo@outlook.com";
        var preferencesMock = CreatePreferencesMock(
            getLastKnownAccountUsername: () => storedAccountUsername,
            setLastKnownAccountUsername: value => storedAccountUsername = value,
            clearLastKnownAccountUsername: () => storedAccountUsername = null);
        var loggerMock = new Mock<ILogger<OneDriveAuthStateStore>>();
        var store = new OneDriveAuthStateStore(preferencesMock.Object, loggerMock.Object);

        store.SetLastKnownAccountUsername("   ");

        store.GetLastKnownAccountUsername().Should().BeNull();
    }

    #endregion

    #region Private Helper Methods

    private static Mock<IPreferences> CreatePreferencesMock(
        Func<string?> getLastKnownAccountUsername,
        Action<string?> setLastKnownAccountUsername,
        Action clearLastKnownAccountUsername)
    {
        var preferencesMock = new Mock<IPreferences>();
        preferencesMock
            .Setup(x => x.Get<string?>("onedrive-auth.last-known-account-username", null, null))
            .Returns(() => getLastKnownAccountUsername());
        preferencesMock
            .Setup(x => x.Set("onedrive-auth.last-known-account-username", It.IsAny<string>(), null))
            .Callback<string, string, string?>((_, value, _) => setLastKnownAccountUsername(value));
        preferencesMock
            .Setup(x => x.Remove("onedrive-auth.last-known-account-username", null))
            .Callback(clearLastKnownAccountUsername);
        return preferencesMock;
    }

    #endregion
}
