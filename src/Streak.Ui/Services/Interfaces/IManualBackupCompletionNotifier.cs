namespace Streak.Ui.Services.Interfaces;

public interface IManualBackupCompletionNotifier
{
    /// <summary>
    ///     Shows in-app feedback after a manual database export completes successfully.
    /// </summary>
    /// <param name="exportResult">The completed export result.</param>
    void NotifyCompleted(DatabaseExportResult exportResult);
}
