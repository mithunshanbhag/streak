#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidAutomatedBackupFileSaver : IAutomatedBackupFileSaver
{
    private static readonly string RelativeDirectoryPath = $"{Environment.DirectoryDownloads}/{AutomatedBackupConstants.SharedAndroidDirectoryName}";

    public Task<SavedFileLocation> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        return AndroidMediaStoreBackupFileWriter.SaveBackupAsync(
            backupFilePath,
            RelativeDirectoryPath,
            cancellationToken);
    }
}
#endif
