namespace Streak.Ui.Services.Interfaces;

public interface IAutomatedBackupRunService
{
    /// <summary>
    ///     Executes the currently enabled nightly automated backup destinations and returns the outcome summary for the run.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The local/cloud execution outcome summary for the nightly run.</returns>
    Task<AutomatedBackupRunResult> ExecuteEnabledBackupsAsync(CancellationToken cancellationToken = default);
}
