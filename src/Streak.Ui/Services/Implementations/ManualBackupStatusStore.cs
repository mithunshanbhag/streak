namespace Streak.Ui.Services.Implementations;

public sealed class ManualBackupStatusStore(IPreferences preferences) : IManualBackupStatusStore
{
    private const string LastSuccessfulLocalBackupUtcKey = "manual-backup.local.last-success-utc";

    private const string LastSuccessfulCloudBackupUtcKey = "manual-backup.cloud.last-success-utc";

    private readonly IPreferences _preferences = preferences;

    public DateTimeOffset? GetLastSuccessfulBackupUtc(ManualBackupLocation location)
    {
        var rawValue = _preferences.Get<string?>(GetPreferenceKey(location), null);
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        if (DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedValue))
            return parsedValue.ToUniversalTime();

        Clear(location);
        return null;
    }

    public void SetLastSuccessfulBackupUtc(ManualBackupLocation location, DateTimeOffset completedAtUtc)
    {
        _preferences.Set(
            GetPreferenceKey(location),
            completedAtUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture));
    }

    public void Clear(ManualBackupLocation location)
    {
        _preferences.Remove(GetPreferenceKey(location));
    }

    private static string GetPreferenceKey(ManualBackupLocation location)
    {
        return location switch
        {
            ManualBackupLocation.Local => LastSuccessfulLocalBackupUtcKey,
            ManualBackupLocation.Cloud => LastSuccessfulCloudBackupUtcKey,
            _ => throw new ArgumentOutOfRangeException(nameof(location), location, "Unsupported manual backup location.")
        };
    }
}
