#if ANDROID
using Environment = Android.OS.Environment;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidAutomatedBackupFileSaver : IAutomatedBackupFileSaver
{
    private static readonly string RelativeDirectoryPath = $"{Environment.DirectoryDownloads}/{AutomatedBackupConstants.SharedAndroidDirectoryName}";

    public Task<string> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        return AndroidMediaStoreBackupFileWriter.SaveFileAsync(
            backupFilePath,
            RelativeDirectoryPath,
            cancellationToken);
    }
}
#endif
