namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveAuthStateStore(IPreferences preferences) : IOneDriveAuthStateStore
{
    private const string LastKnownAccountUsernameKey = "onedrive-auth.last-known-account-username";

    private readonly IPreferences _preferences = preferences;

    public string? GetLastKnownAccountUsername()
    {
        var accountUsername = _preferences.Get<string?>(LastKnownAccountUsernameKey, null);
        return string.IsNullOrWhiteSpace(accountUsername)
            ? null
            : accountUsername;
    }

    public void SetLastKnownAccountUsername(string? accountUsername)
    {
        if (string.IsNullOrWhiteSpace(accountUsername))
        {
            Clear();
            return;
        }

        _preferences.Set(LastKnownAccountUsernameKey, accountUsername);
    }

    public void Clear()
    {
        _preferences.Remove(LastKnownAccountUsernameKey);
    }
}
