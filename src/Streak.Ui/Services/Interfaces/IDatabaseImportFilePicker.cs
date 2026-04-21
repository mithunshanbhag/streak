namespace Streak.Ui.Services.Interfaces;

public interface IDatabaseImportFilePicker
{
    /// <summary>
    ///     Opens a native file picker for selecting a Streak data-backup archive.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     The selected backup file when the user confirms the picker; otherwise <see langword="null" /> when the user
    ///     cancels.
    /// </returns>
    Task<FileResult?> PickBackupAsync(CancellationToken cancellationToken = default);
}
