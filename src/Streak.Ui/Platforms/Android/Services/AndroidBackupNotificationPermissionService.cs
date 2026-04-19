#if ANDROID
namespace Streak.Ui.Services.Implementations;

public sealed class AndroidBackupNotificationPermissionService : IBackupNotificationPermissionService
{
    public async Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            return true;

        var status = await Permissions.CheckStatusAsync<Permissions.PostNotifications>();
        if (status == PermissionStatus.Granted)
            return true;

        cancellationToken.ThrowIfCancellationRequested();

        status = await Permissions.RequestAsync<Permissions.PostNotifications>();
        return status == PermissionStatus.Granted;
    }
}
#endif
