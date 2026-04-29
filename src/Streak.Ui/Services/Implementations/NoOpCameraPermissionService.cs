namespace Streak.Ui.Services.Implementations;

public sealed class NoOpCameraPermissionService : ICameraPermissionService
{
    public Task<bool> RequestPermissionIfNeededAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(true);
    }
}
