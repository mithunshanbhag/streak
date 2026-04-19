namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedBackupFolderOpener : IBackupFolderOpener
{
    public bool CanOpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        return false;
    }

    public void OpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        throw new NotSupportedException("Opening backup folders is not supported on this platform.");
    }
}
