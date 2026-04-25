namespace Streak.Ui.Exceptions;

public sealed class OneDriveBackupException(
    OneDriveBackupFailureKind failureKind,
    string message,
    Exception? innerException = null)
    : Exception(message, innerException)
{
    public OneDriveBackupFailureKind FailureKind { get; } = failureKind;
}
