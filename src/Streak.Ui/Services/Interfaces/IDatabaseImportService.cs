namespace Streak.Ui.Services.Interfaces;

public interface IDatabaseImportService
{
    /// <summary>
    ///     Replaces the live Streak database with the selected backup file.
    /// </summary>
    /// <param name="backupFile">The backup file selected by the user.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task ImportDatabaseAsync(FileResult backupFile, CancellationToken cancellationToken = default);
}