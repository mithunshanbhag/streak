namespace Streak.Ui.Services.Implementations;

public sealed class ManualBackupStatusStore(
    IPreferences preferences,
    ILogger<ManualBackupStatusStore> logger)
    : IManualBackupStatusStore
{
    private const string LastSuccessfulLocalBackupUtcKey = "manual-backup.local.last-success-utc";

    private const string LastSuccessfulCloudBackupUtcKey = "manual-backup.cloud.last-success-utc";

    private readonly IPreferences _preferences = preferences;
    private readonly ILogger<ManualBackupStatusStore> _logger = logger;

    public DateTimeOffset? GetLastSuccessfulBackupUtc(ManualBackupLocation location)
    {
        var rawValue = _preferences.Get<string?>(GetPreferenceKey(location), null);
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;

        if (DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsedValue))
            return parsedValue.ToUniversalTime();

        _logger.LogWarning(
            "Ignoring invalid persisted manual backup timestamp for {BackupLocation}; clearing stored value.",
            location);
        Clear(location);
        return null;
    }

    public void SetLastSuccessfulBackupUtc(ManualBackupLocation location, DateTimeOffset completedAtUtc)
    {
        var normalizedCompletedAtUtc = completedAtUtc.ToUniversalTime();

        _preferences.Set(
            GetPreferenceKey(location),
            normalizedCompletedAtUtc.ToString("O", CultureInfo.InvariantCulture));

        _logger.LogInformation(
            "Persisted successful manual {BackupLocation} backup timestamp at {CompletedAtUtc}.",
            location,
            normalizedCompletedAtUtc);
    }

    public void Clear(ManualBackupLocation location)
    {
        _logger.LogInformation("Clearing persisted manual {BackupLocation} backup timestamp.", location);
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
