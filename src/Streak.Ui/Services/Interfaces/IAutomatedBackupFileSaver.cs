namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupFileSaver
{
    /// <summary>
    ///     Persists the generated automated-backup file into the platform's shared backup location.
    /// </summary>
    /// <param name="backupFilePath">The temporary generated backup file to persist.</param>
    /// <param name="cancellationToken">Cancels the write before it completes.</param>
    /// <returns>The saved backup file and parent folder details.</returns>
    Task<SavedFileLocation> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default);
}
