namespace Streak.Ui.Constants;

public static class AutomatedBackupConstants
{
    public const int SettingsRowId = 1;

    public const int ScheduledHour = 23;

    public const int ScheduledMinute = 30;

    public const int AlarmRequestCode = 1130;

    public const string AlarmAction = "com.companyname.streak.ui.action.AUTOMATED_BACKUP_TRIGGER";

    public const string SettingsTableName = "AutomatedBackupSettings";

    public const string LocalEnabledColumnName = "IsEnabled";

    public const string CloudEnabledColumnName = "IsCloudEnabled";

    public const string SharedAndroidDirectoryName = StreakExportStorageConstants.AndroidRootDirectoryName;
}
