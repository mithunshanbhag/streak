namespace Streak.Ui.Services.Interfaces;

public interface IBackupArchiveFactory
{
    /// <summary>
    ///     Creates a temporary manual backup archive that contains the current SQLite database snapshot and any available proof files.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     The temporary backup archive artifact. The caller should dispose it when the working file is no longer needed so temporary files are cleaned up.
    /// </returns>
    /// <exception cref="FileNotFoundException">Thrown when the live SQLite database file does not exist.</exception>
    Task<BackupArchiveArtifact> CreateManualBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Creates a temporary automated backup archive that contains the current SQLite database snapshot and any available proof files.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>
    ///     The temporary backup archive artifact. The caller should dispose it when the working file is no longer needed so temporary files are cleaned up.
    /// </returns>
    /// <exception cref="FileNotFoundException">Thrown when the live SQLite database file does not exist.</exception>
    Task<BackupArchiveArtifact> CreateAutomatedBackupAsync(CancellationToken cancellationToken = default);
}
