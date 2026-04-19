namespace Streak.Ui.Services.Implementations;

public sealed class NoOpBackupNotificationPermissionService : IBackupNotificationPermissionService
{
    public Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }
}
