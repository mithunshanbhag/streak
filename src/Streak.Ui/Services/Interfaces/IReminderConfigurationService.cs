namespace Streak.Ui.Services.Interfaces;

public interface IReminderConfigurationService
{
    /// <summary>
    ///     Gets the persisted daily reminder enabled state.
    /// </summary>
    /// <returns><see langword="true" /> when daily reminders are enabled; otherwise, <see langword="false" />.</returns>
    bool GetIsEnabled();

    /// <summary>
    ///     Gets the persisted local reminder time.
    /// </summary>
    /// <returns>The local time of day used for the next reminder trigger.</returns>
    TimeOnly GetTimeLocal();

    /// <summary>
    ///     Persists the daily reminder enabled state and synchronizes operating system scheduling to match it.
    /// </summary>
    /// <param name="isEnabled">
    ///     <see langword="true" /> to enable daily reminders; otherwise, <see langword="false" />.
    /// </param>
    void SetIsEnabled(bool isEnabled);

    /// <summary>
    ///     Persists the daily reminder local time and synchronizes operating system scheduling to match it.
    /// </summary>
    /// <param name="timeLocal">The local time of day for the reminder.</param>
    void SetTimeLocal(TimeOnly timeLocal);

    /// <summary>
    ///     Reads the persisted reminder settings and synchronizes operating system registration to match them.
    /// </summary>
    void SynchronizeScheduler();
}
