namespace Streak.Ui.Services.Models;

public enum OneDriveBackupFailureKind
{
    AuthRequired = 1,
    NetworkUnavailable = 2,
    QuotaExceeded = 3,
    AccessDenied = 4,
    Unknown = 5
}
