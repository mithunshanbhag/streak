#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        await AndroidMediaStoreBackupFileWriter.SaveFileAsync(
            backupFilePath,
            Environment.DirectoryDownloads,
            cancellationToken);

        return DatabaseExportResult.Saved;
    }
}
#endif
