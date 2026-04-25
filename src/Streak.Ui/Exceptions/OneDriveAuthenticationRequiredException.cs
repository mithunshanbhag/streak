namespace Streak.Ui.Exceptions;

public sealed class OneDriveAuthenticationRequiredException(string message, Exception? innerException = null)
    : Exception(message, innerException);
