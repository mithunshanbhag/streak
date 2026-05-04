namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveAuthStateStore(
    IPreferences preferences,
    ILogger<OneDriveAuthStateStore> logger)
    : IOneDriveAuthStateStore
{
    private const string LastKnownAccountUsernameKey = "onedrive-auth.last-known-account-username";

    private readonly IPreferences _preferences = preferences;
    private readonly ILogger<OneDriveAuthStateStore> _logger = logger;

    public string? GetLastKnownAccountUsername()
    {
        var accountUsername = _preferences.Get<string?>(LastKnownAccountUsernameKey, null);
        var hasAccountUsername = !string.IsNullOrWhiteSpace(accountUsername);

        _logger.LogDebug(
            "Loaded persisted OneDrive account state. Username present: {HasAccountUsername}.",
            hasAccountUsername);

        return hasAccountUsername ? accountUsername : null;
    }

    public void SetLastKnownAccountUsername(string? accountUsername)
    {
        if (string.IsNullOrWhiteSpace(accountUsername))
        {
            _logger.LogInformation(
                "Clearing persisted OneDrive account username because the provided value was blank.");
            Clear();
            return;
        }

        _preferences.Set(LastKnownAccountUsernameKey, accountUsername);
        _logger.LogInformation("Persisted OneDrive account username for reconnect state.");
    }

    public void Clear()
    {
        _logger.LogInformation("Clearing persisted OneDrive account username.");
        _preferences.Remove(LastKnownAccountUsernameKey);
    }
}
