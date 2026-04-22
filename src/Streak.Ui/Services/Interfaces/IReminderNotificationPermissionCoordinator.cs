namespace Streak.Ui.Services.Interfaces;

public interface IReminderNotificationPermissionCoordinator
{
    /// <summary>
    ///     Requests reminder notification permission when daily reminders are currently enabled.
    /// </summary>
    /// <param name="cancellationToken">Cancels the permission request.</param>
    /// <returns>
    ///     <see langword="true" /> when reminders are disabled or reminder notifications can be shown after
    ///     the request; otherwise, <see langword="false" />.
    /// </returns>
    Task<bool> RequestPermissionIfRemindersEnabledAsync(CancellationToken cancellationToken = default);
}
