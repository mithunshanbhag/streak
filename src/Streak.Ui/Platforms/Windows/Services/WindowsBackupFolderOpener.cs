#if WINDOWS
using System.Diagnostics;

namespace Streak.Ui.Services.Implementations;

public sealed class WindowsBackupFolderOpener : IBackupFolderOpener
{
    public bool CanOpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        return folderKind == BackupFolderKind.ManualExport
               && !string.IsNullOrWhiteSpace(savedFileLocation?.SavedFilePath);
    }

    public void OpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        if (!CanOpenFolder(folderKind, savedFileLocation))
            throw new InvalidOperationException("A saved Windows backup file path is required to open the containing folder.");

        var startInfo = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"/select,\"{savedFileLocation!.SavedFilePath}\"",
            UseShellExecute = true
        };

        _ = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Windows Explorer did not start when opening the backup folder.");
    }
}
#endif
