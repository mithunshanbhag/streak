namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedOneDriveBackupUploadClient : IOneDriveBackupUploadClient
{
    public Task UploadManualBackupAsync(
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("OneDrive backup upload is not supported on this platform.");
    }
}
