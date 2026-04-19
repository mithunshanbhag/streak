namespace Streak.Ui.Constants;

public static class BackupNotificationConstants
{
    public const string AndroidChannelId = "streak.backup.completions";

    public const string AndroidChannelName = "Backup completions";

    public const string AndroidChannelDescription = "Notifications when Streak finishes saving nightly automated backups.";

    public const int AutomatedBackupNotificationId = 2001;

    public const string OpenFolderAction = "com.companyname.streak.ui.action.OPEN_BACKUP_FOLDER";

    public const string FolderKindExtraKey = "backup_folder_kind";
}
