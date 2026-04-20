#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidAutomatedBackupFileSaver : IAutomatedBackupFileSaver
{
    private static readonly string RelativeDirectoryPath = string.Join(
        '/',
        Environment.DirectoryDownloads,
        AutomatedBackupConstants.SharedAndroidDirectoryName,
        StreakExportStorageConstants.BackupsDirectoryName,
        StreakExportStorageConstants.AutomatedBackupsDirectoryName);

    public Task<SavedFileLocation> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        return AndroidMediaStoreBackupFileWriter.SaveBackupAsync(
            backupFilePath,
            RelativeDirectoryPath,
            StreakExportStorageConstants.AutomatedBackupsDisplayDirectoryPath,
            cancellationToken);
    }
}
#endif
