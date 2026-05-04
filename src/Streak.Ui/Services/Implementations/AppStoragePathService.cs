namespace Streak.Ui.Services.Implementations;

public sealed class AppStoragePathService : IAppStoragePathService
{
    public string DatabasePath => SqliteDatabaseBootstrapper.DatabasePath;

    public string ExportDirectoryPath => Path.Combine(
        FileSystem.Current.CacheDirectory,
        StreakExportStorageConstants.ExportWorkingDirectoryName);

    public string CheckinProofsDirectoryPath =>
#if WINDOWS
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
            StreakExportStorageConstants.AndroidRootDirectoryName,
            CheckinProofStorageConstants.CheckinProofsDirectoryName);
#elif ANDROID
        Path.Combine(
            "/storage/emulated/0",
            Android.OS.Environment.DirectoryPictures,
            StreakExportStorageConstants.AndroidRootDirectoryName,
            CheckinProofStorageConstants.CheckinProofsDirectoryName);
#else
        Path.Combine(
            FileSystem.Current.AppDataDirectory,
            CheckinProofStorageConstants.CheckinProofsDirectoryName);
#endif
}
