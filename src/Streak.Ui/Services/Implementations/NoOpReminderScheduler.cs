namespace Streak.Ui.Services.Implementations;

public sealed class NoOpReminderScheduler : IReminderScheduler
{
    public void Synchronize(bool isEnabled, TimeOnly timeLocal)
    {
    }
}
