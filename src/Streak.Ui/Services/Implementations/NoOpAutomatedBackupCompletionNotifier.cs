namespace Streak.Ui.Services.Implementations;

public sealed class NoOpAutomatedBackupCompletionNotifier : IAutomatedBackupCompletionNotifier
{
    public void NotifyCompleted(SavedFileLocation savedFileLocation)
    {
        ArgumentNullException.ThrowIfNull(savedFileLocation);
    }
}
