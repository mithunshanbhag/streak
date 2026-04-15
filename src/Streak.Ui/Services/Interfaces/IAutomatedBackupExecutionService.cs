namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupExecutionService
{
    /// <summary>
    ///     Creates a standalone SQLite backup copy and persists it to the shared automated-backup location.
    /// </summary>
    /// <param name="cancellationToken">Cancels the backup before it completes.</param>
    /// <returns>A human-readable saved location for logging.</returns>
    Task<string> ExecuteAutomatedBackupAsync(CancellationToken cancellationToken = default);
}
