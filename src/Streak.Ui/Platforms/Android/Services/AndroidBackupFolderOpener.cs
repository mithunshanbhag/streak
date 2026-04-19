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

        var intent = folderKind == BackupFolderKind.AutomatedBackup
            ? CreateAutomatedBackupFolderIntent()
            : CreateDownloadsIntent();

        try
        {
            Application.Context.StartActivity(intent);
        }
        catch (ActivityNotFoundException) when (folderKind == BackupFolderKind.AutomatedBackup)
        {
            Application.Context.StartActivity(CreateDownloadsIntent());
        }
        catch (Java.Lang.SecurityException) when (folderKind == BackupFolderKind.AutomatedBackup)
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

    private static Intent CreateAutomatedBackupFolderIntent()
    {
        var folderUri = Uri.Parse("content://com.android.externalstorage.documents/document/primary%3ADownload%2FStreak");
        var intent = new Intent(Intent.ActionView);
        intent.SetDataAndType(folderUri, DocumentsContract.Document.MimeTypeDir);
        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);
        return intent;
    }
}
#endif
