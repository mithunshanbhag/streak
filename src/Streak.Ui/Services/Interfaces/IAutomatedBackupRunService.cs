namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupRunService
{
    /// <summary>
    ///     Executes the nightly automated local-backup destination when it is currently enabled and returns the outcome summary for that destination.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The local-backup execution outcome summary.</returns>
    Task<AutomatedBackupRunResult> ExecuteEnabledLocalBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the nightly automated OneDrive backup destination when it is currently enabled and returns the outcome summary for that destination.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The OneDrive-backup execution outcome summary.</returns>
    Task<AutomatedBackupRunResult> ExecuteEnabledCloudBackupAsync(CancellationToken cancellationToken = default);

    /// <summary>
    ///     Executes the currently enabled nightly automated backup destinations and returns the outcome summary for the run.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The local/cloud execution outcome summary for the nightly run.</returns>
    Task<AutomatedBackupRunResult> ExecuteEnabledBackupsAsync(CancellationToken cancellationToken = default);
}
