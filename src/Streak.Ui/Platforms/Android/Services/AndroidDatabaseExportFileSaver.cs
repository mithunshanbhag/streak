#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        var savedFileLocation = await AndroidMediaStoreBackupFileWriter.SaveBackupAsync(
            backupFilePath,
            Environment.DirectoryDownloads,
            cancellationToken);

        return DatabaseExportResult.Saved(savedFileLocation);
    }
}
#endif
