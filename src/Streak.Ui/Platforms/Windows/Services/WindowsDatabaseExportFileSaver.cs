#if WINDOWS
namespace Streak.Ui.Services.Implementations;

public sealed class WindowsDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        var wasSaved = await WindowsFileSavePickerExportUtility.SaveFileAsync(
            backupFilePath,
            "SQLite database backup",
            ".db",
            cancellationToken);

        return wasSaved
            ? DatabaseExportResult.Saved
            : DatabaseExportResult.Cancelled;
    }
}
#endif
