namespace Streak.Ui.Services.Interfaces;

public interface IReminderNotificationPermissionService
{
    /// <summary>
    ///     Requests any runtime permission needed for reminder notifications on the current platform.
    /// </summary>
    /// <param name="cancellationToken">Cancels the permission request.</param>
    /// <returns>
    ///     <see langword="true" /> when reminder notifications can be shown after the request; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default);
}
