namespace Streak.Ui.Services.Implementations;

public sealed class AppStoragePathService : IAppStoragePathService
{
    public string DatabasePath => SqliteDatabaseBootstrapper.DatabasePath;

    public string ExportDirectoryPath => Path.Combine(
        FileSystem.Current.CacheDirectory,
        StreakExportStorageConstants.ExportWorkingDirectoryName);

    public string DiagnosticsDirectoryPath => DiagnosticsStoragePathHelper.GetDiagnosticsDirectoryPath(FileSystem.Current.AppDataDirectory);

    public string DiagnosticsLogFilePath => DiagnosticsStoragePathHelper.GetDiagnosticsLogFilePath(FileSystem.Current.AppDataDirectory);
}
