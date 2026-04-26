namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedAutomatedCloudBackupService : IAutomatedCloudBackupService
{
    public Task UploadAutomatedBackupAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Automated OneDrive backup is not supported on this platform.");
    }
}
