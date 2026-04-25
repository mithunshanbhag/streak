namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedManualCloudBackupService : IManualCloudBackupService
{
    public Task UploadManualBackupAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Manual OneDrive backup is not supported on this platform.");
    }
}
