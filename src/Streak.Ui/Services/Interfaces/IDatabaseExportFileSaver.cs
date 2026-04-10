namespace Streak.Ui.Services.Interfaces;

public interface IDatabaseExportFileSaver
{
    Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default);
}