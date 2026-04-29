#if ANDROID
namespace Streak.Ui.Services.Implementations;

public sealed class AndroidCameraPermissionService : ICameraPermissionService
{
    public async Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
            return true;

        cancellationToken.ThrowIfCancellationRequested();

        status = await Permissions.RequestAsync<Permissions.Camera>();
        return status == PermissionStatus.Granted;
    }
}
#endif
