namespace Streak.Ui.Services.Interfaces;

public interface IPostStartupPermissionRecoveryCoordinator
{
    /// <summary>
    ///     Reconciles any missing runtime permissions that should be recovered after the Homepage has rendered.
    /// </summary>
    /// <param name="cancellationToken">Cancels the permission recovery operation.</param>
    Task RecoverMissingPermissionsAfterHomepageRenderAsync(CancellationToken cancellationToken = default);
}
