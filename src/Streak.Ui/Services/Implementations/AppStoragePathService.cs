namespace Streak.Ui.Services.Implementations;

public sealed class AppStoragePathService : IAppStoragePathService
{
    public string DatabasePath => SqliteDatabaseBootstrapper.DatabasePath;

    public string ExportDirectoryPath => FileSystem.Current.CacheDirectory;
}