namespace Streak.Ui.Services.Interfaces;

public interface ICameraPermissionService
{
    /// <summary>
    ///     Requests any runtime permission needed for camera capture on the current platform.
    /// </summary>
    /// <param name="cancellationToken">Cancels the permission request.</param>
    /// <returns>
    ///     <see langword="true" /> when camera capture can proceed after the request; otherwise,
    ///     <see langword="false" />.
    /// </returns>
    Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default);
}
