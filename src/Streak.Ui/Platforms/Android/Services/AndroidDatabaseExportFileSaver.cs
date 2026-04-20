#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    private static readonly string RelativeDirectoryPath = string.Join(
        '/',
        Environment.DirectoryDownloads,
        StreakExportStorageConstants.AndroidRootDirectoryName,
        StreakExportStorageConstants.BackupsDirectoryName,
        StreakExportStorageConstants.ManualBackupsDirectoryName);

    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        var savedFileLocation = await AndroidMediaStoreBackupFileWriter.SaveBackupAsync(
            backupFilePath,
            RelativeDirectoryPath,
            StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath,
            cancellationToken);

        return DatabaseExportResult.Saved(savedFileLocation);
    }
}
#endif
