namespace Streak.Ui.Services.Implementations;

public sealed class NoOpAutomatedBackupScheduler : IAutomatedBackupScheduler
{
    public void Synchronize(bool isEnabled)
    {
    }
}
