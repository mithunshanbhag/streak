namespace Streak.Ui.Services.Interfaces;

public interface IManualCloudBackupService
{
    /// <summary>
    ///     Creates a fresh backup archive from the current local data and uploads it to the connected OneDrive app folder.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="OneDriveBackupException">
    ///     Thrown when the upload fails because reconnect, quota recovery, or network retry is required.
    /// </exception>
    Task UploadManualBackupAsync(CancellationToken cancellationToken = default);
}
