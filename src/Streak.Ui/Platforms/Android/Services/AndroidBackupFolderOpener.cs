#if ANDROID
using Android.App;
using Android.Content;
using Android.Provider;
using Uri = Android.Net.Uri;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidBackupFolderOpener : IBackupFolderOpener
{
    public bool CanOpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        return folderKind is BackupFolderKind.ManualExport or BackupFolderKind.AutomatedBackup;
    }

    public void OpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        if (!CanOpenFolder(folderKind, savedFileLocation))
            throw new NotSupportedException($"Opening {folderKind} folders is not supported on Android.");

        var intent = CreateBackupFolderIntent(folderKind);

        try
        {
            Application.Context.StartActivity(intent);
        }
        catch (ActivityNotFoundException)
        {
            Application.Context.StartActivity(CreateDownloadsIntent());
        }
        catch (Java.Lang.SecurityException)
        {
            Application.Context.StartActivity(CreateDownloadsIntent());
        }
    }

    private static Intent CreateDownloadsIntent()
    {
        var intent = new Intent(DownloadManager.ActionViewDownloads);
        intent.AddFlags(ActivityFlags.NewTask);
        return intent;
    }

    private static Intent CreateBackupFolderIntent(BackupFolderKind folderKind)
    {
        var folderUri = Uri.Parse($"content://com.android.externalstorage.documents/document/{CreateEncodedDocumentId(folderKind)}");
        var intent = new Intent(Intent.ActionView);
        intent.SetDataAndType(folderUri, DocumentsContract.Document.MimeTypeDir);
        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);
        return intent;
    }

    private static string CreateEncodedDocumentId(BackupFolderKind folderKind)
    {
        var relativePath = folderKind switch
        {
            BackupFolderKind.ManualExport => string.Join(
                '/',
                Android.OS.Environment.DirectoryDownloads,
                StreakExportStorageConstants.AndroidRootDirectoryName,
                StreakExportStorageConstants.BackupsDirectoryName,
                StreakExportStorageConstants.ManualBackupsDirectoryName),
            BackupFolderKind.AutomatedBackup => string.Join(
                '/',
                Android.OS.Environment.DirectoryDownloads,
                StreakExportStorageConstants.AndroidRootDirectoryName,
                StreakExportStorageConstants.BackupsDirectoryName,
                StreakExportStorageConstants.AutomatedBackupsDirectoryName),
            _ => throw new NotSupportedException($"Opening {folderKind} folders is not supported on Android.")
        };

        return global::System.Uri.EscapeDataString($"primary:{relativePath}");
    }
}
#endif
