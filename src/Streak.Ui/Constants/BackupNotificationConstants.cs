namespace Streak.Ui.Constants;

public static class BackupNotificationConstants
{
    public const string AndroidChannelId = "streak.backup.completions";

    public const string AndroidChannelName = "Backup notifications";

    public const string AndroidChannelDescription = "Notifications when Streak finishes or cannot complete nightly automated backups.";

    public const int AutomatedBackupNotificationId = 2001;

    public const int AutomatedBackupForegroundServiceNotificationId = 2002;

    public const int AutomatedBackupFailureNotificationId = 2003;

    public const string OpenFolderAction = "com.companyname.streak.ui.action.OPEN_BACKUP_FOLDER";

    public const string OpenAppFromFailureAction = "com.companyname.streak.ui.action.OPEN_APP_FROM_BACKUP_FAILURE";

    public const string FolderKindExtraKey = "backup_folder_kind";
}
