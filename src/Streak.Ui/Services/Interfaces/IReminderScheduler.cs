namespace Streak.Ui.Services.Interfaces;

public interface IReminderScheduler
{
    /// <summary>
    ///     Synchronizes operating system registration for daily reminders with the supplied settings.
    /// </summary>
    /// <param name="isEnabled">
    ///     <see langword="true" /> to ensure the local reminder trigger is registered; otherwise,
    ///     <see langword="false" /> to remove future reminder triggers.
    /// </param>
    /// <param name="timeLocal">The local time of day when the reminder should fire.</param>
    void Synchronize(bool isEnabled, TimeOnly timeLocal);
}
