namespace Streak.Ui.Services.Interfaces;

public interface IAppStoragePathService
{
    /// <summary>
    ///     Gets the full path to the live SQLite database file.
    /// </summary>
    string DatabasePath { get; }

    /// <summary>
    ///     Gets the cache-backed working directory used for temporary export and share artifacts.
    /// </summary>
    string ExportDirectoryPath { get; }

    /// <summary>
    ///     Gets the root directory or display path used for saved check-in proof files.
    /// </summary>
    string CheckinProofsDirectoryPath { get; }
}
