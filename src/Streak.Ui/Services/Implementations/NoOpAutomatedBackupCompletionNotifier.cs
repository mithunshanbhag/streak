namespace Streak.Ui.Services.Implementations;

public sealed class NoOpAutomatedBackupCompletionNotifier : IAutomatedBackupCompletionNotifier
{
    public void NotifyCompleted(SavedFileLocation savedFileLocation)
    {
        ArgumentNullException.ThrowIfNull(savedFileLocation);
    }

    public void NotifyFailed(AutomatedBackupRunResult runResult)
    {
        ArgumentNullException.ThrowIfNull(runResult);
    }
}
