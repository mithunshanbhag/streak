namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedCloudBackupService
{
    /// <summary>
    ///     Creates a fresh automated backup archive from the current local data and uploads it to the connected OneDrive app folder automated-backup location.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="OneDriveBackupException">
    ///     Thrown when the upload fails because reconnect, quota recovery, or network retry is required.
    /// </exception>
    Task UploadAutomatedBackupAsync(CancellationToken cancellationToken = default);
}
