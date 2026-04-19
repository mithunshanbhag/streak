namespace Streak.Ui.Services.Interfaces;

public interface IBackupNotificationPermissionService
{
    /// <summary>
    ///     Requests any runtime permission needed for backup-completion notifications on the current platform.
    /// </summary>
    /// <param name="cancellationToken">Cancels the permission request.</param>
    /// <returns>
    ///     <see langword="true" /> when backup notifications can be shown after the request, otherwise
    ///     <see langword="false" />.
    /// </returns>
    Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default);
}
