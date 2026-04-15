namespace Streak.Ui.Services.Implementations;

public sealed class NoOpAutomatedBackupScheduler : IAutomatedBackupScheduler
{
    public bool IsSupported => false;

    public void Synchronize(bool isEnabled)
    {
    }
}
