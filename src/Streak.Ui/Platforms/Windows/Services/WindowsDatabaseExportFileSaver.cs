#if WINDOWS
namespace Streak.Ui.Services.Implementations;

public sealed class WindowsDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        var savedFileLocation = await WindowsFileSavePickerExportUtility.SaveFileAsync(
            backupFilePath,
            "SQLite database backup",
            ".db",
            cancellationToken);

        return savedFileLocation is not null
            ? DatabaseExportResult.Saved(savedFileLocation)
            : DatabaseExportResult.Cancelled;
    }
}
#endif
