#if ANDROID
using Android.App;
using Android.Content;
using Android.Provider;
using Uri = Android.Net.Uri;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidBackupFolderOpener : IBackupFolderOpener
{
    private const string DocumentsUiFilesActivityClassName = "com.android.documentsui.files.FilesActivity";

    private static readonly string[] DocumentsUiPackageNames =
    [
        "com.google.android.documentsui",
        "com.android.documentsui"
    ];

    public bool CanOpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        return folderKind is BackupFolderKind.ManualExport or BackupFolderKind.AutomatedBackup;
    }

    public void OpenFolder(BackupFolderKind folderKind, SavedFileLocation? savedFileLocation = null)
    {
        if (!CanOpenFolder(folderKind, savedFileLocation))
            throw new NotSupportedException($"Opening {folderKind} folders is not supported on Android.");

        foreach (var intent in CreateBackupFolderIntents(folderKind))
        {
            try
            {
                Application.Context.StartActivity(intent);
                return;
            }
            catch (ActivityNotFoundException)
            {
                // Try the next folder-opening strategy before falling back to Downloads.
            }
            catch (Java.Lang.SecurityException)
            {
                // Some OEM file managers reject direct document URIs without a persisted tree grant.
            }
        }

        Application.Context.StartActivity(CreateDownloadsIntent());
    }

    private static Intent CreateDownloadsIntent()
    {
        var intent = new Intent(DownloadManager.ActionViewDownloads);
        intent.AddFlags(ActivityFlags.NewTask);
        return intent;
    }

    private static IEnumerable<Intent> CreateBackupFolderIntents(BackupFolderKind folderKind)
    {
        var folderUri = DocumentsContract.BuildDocumentUri(
                            "com.android.externalstorage.documents",
                            CreateDocumentId(folderKind))
                        ?? throw new InvalidOperationException("Unable to create an Android folder URI for the backup location.");

        foreach (var packageName in DocumentsUiPackageNames)
            yield return CreateDocumentsUiFolderIntent(folderUri, packageName);
    }

    private static Intent CreateDocumentsUiFolderIntent(Uri folderUri, string packageName)
    {
        var intent = CreateGenericFolderIntent(folderUri);
        intent.SetComponent(new ComponentName(packageName, DocumentsUiFilesActivityClassName));
        return intent;
    }

    private static Intent CreateGenericFolderIntent(Uri folderUri)
    {
        var intent = new Intent(Intent.ActionView);
        intent.SetData(folderUri);
        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.GrantReadUriPermission);
        return intent;
    }

    private static string CreateDocumentId(BackupFolderKind folderKind)
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

        return $"primary:{relativePath}";
    }
}
#endif
