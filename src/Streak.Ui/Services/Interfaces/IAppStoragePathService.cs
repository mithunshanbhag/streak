namespace Streak.Ui.Services.Interfaces;

public interface IAppStoragePathService
{
    string DatabasePath { get; }

    string ExportDirectoryPath { get; }
}