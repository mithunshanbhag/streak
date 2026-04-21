namespace Streak.Ui.Services.Interfaces;

public interface ICheckinProofMediaPickerService
{
    /// <summary>
    ///     Gets a value indicating whether the current platform supports capturing a photo from the system camera UI.
    /// </summary>
    bool SupportsCameraCapture { get; }

    /// <summary>
    ///     Opens the system camera UI to capture a check-in proof photo.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The captured photo when the user confirms; otherwise <see langword="null" /> when the user cancels.</returns>
    Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Opens the system picker UI to choose an existing check-in proof photo.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The selected photo when the user confirms; otherwise <see langword="null" /> when the user cancels.</returns>
    Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default);
}
