namespace Streak.Ui.Services.Interfaces;

public interface ICheckinProofService
{
    /// <summary>
    ///     Gets a value indicating whether the current platform supports capturing a photo from the system camera UI.
    /// </summary>
    bool SupportsCameraCapture { get; }

    /// <summary>
    ///     Captures a new check-in proof photo and returns preview-ready selection metadata.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     The preview-ready selection when the user confirms; otherwise <see langword="null" /> when the user cancels.
    /// </returns>
    Task<CheckinProofSelection?> CapturePhotoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Picks an existing check-in proof photo and returns preview-ready selection metadata.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     The preview-ready selection when the user confirms; otherwise <see langword="null" /> when the user cancels.
    /// </returns>
    Task<CheckinProofSelection?> PickPhotoAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Persists a selected proof photo into app-private storage and returns the metadata that should be saved with the check-in.
    /// </summary>
    /// <param name="selection">The selected proof photo.</param>
    /// <param name="habitId">The habit identifier.</param>
    /// <param name="checkinDate">The local check-in date in <c>yyyy-MM-dd</c> format.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The proof metadata to persist with the check-in.</returns>
    Task<CheckinProofInputModel> PersistAsync(
        CheckinProofSelection selection,
        int habitId,
        string checkinDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes a previously-persisted proof photo when it exists.
    /// </summary>
    /// <param name="proofImageUri">The persisted proof URI relative to the proof-storage root.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task DeleteIfExistsAsync(string? proofImageUri, CancellationToken cancellationToken = default);
}
