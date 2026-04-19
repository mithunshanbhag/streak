namespace Streak.Ui.Services.Interfaces;

public interface IBackupFolderOpener
{
    /// <summary>
    ///     Gets whether the current platform can open the requested backup folder target.
    /// </summary>
    /// <param name="folderKind">The kind of backup folder the user wants to open.</param>
    /// <param name="savedFileLocation">
    ///     Optional saved-file details. Manual exports can supply the saved file path when the platform needs it.
    /// </param>
    bool CanOpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null);

    /// <summary>
    ///     Opens the requested backup folder target using the current platform's native shell.
    /// </summary>
    /// <param name="folderKind">The kind of backup folder the user wants to open.</param>
    /// <param name="savedFileLocation">
    ///     Optional saved-file details. Manual exports can supply the saved file path when the platform needs it.
    /// </param>
    void OpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null);
}
