namespace Streak.Ui.Services.Interfaces;

public interface IOneDriveBackupUploadClient
{
    /// <summary>
    ///     Uploads the specified backup archive into the OneDrive app folder manual-backup location.
    /// </summary>
    /// <param name="localFilePath">The local backup archive path to upload.</param>
    /// <param name="destinationFileName">The destination file name to create under the OneDrive manual-backup folder.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="FileNotFoundException">Thrown when the local backup archive does not exist.</exception>
    /// <exception cref="OneDriveBackupException">
    ///     Thrown when the upload fails because reconnect, quota recovery, or network retry is required.
    /// </exception>
    Task UploadManualBackupAsync(
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Uploads the specified backup archive into the OneDrive app folder automated-backup location.
    /// </summary>
    /// <param name="localFilePath">The local backup archive path to upload.</param>
    /// <param name="destinationFileName">The destination file name to create under the OneDrive automated-backup folder.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="FileNotFoundException">Thrown when the local backup archive does not exist.</exception>
    /// <exception cref="OneDriveBackupException">
    ///     Thrown when the upload fails because reconnect, quota recovery, or network retry is required.
    /// </exception>
    Task UploadAutomatedBackupAsync(
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken = default);
}
