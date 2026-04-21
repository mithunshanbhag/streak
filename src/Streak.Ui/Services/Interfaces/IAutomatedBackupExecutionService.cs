namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupExecutionService
{
    /// <summary>
    ///     Creates a standalone data-backup archive and persists it to the shared automated-backup location.
    /// </summary>
    /// <param name="cancellationToken">Cancels the backup before it completes.</param>
    /// <returns>The saved backup file and parent folder details.</returns>
    Task<SavedFileLocation> ExecuteAutomatedBackupAsync(CancellationToken cancellationToken = default);
}
