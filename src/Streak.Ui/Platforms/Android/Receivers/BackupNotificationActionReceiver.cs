#if ANDROID
using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class BackupNotificationActionReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var logger = services.GetRequiredService<ILogger<BackupNotificationActionReceiver>>();
        var backupFolderOpener = services.GetRequiredService<IBackupFolderOpener>();

        var folderKindValue = intent?.GetStringExtra(BackupNotificationConstants.FolderKindExtraKey);
        if (!Enum.TryParse(folderKindValue, out BackupFolderKind folderKind))
        {
            logger.LogWarning(
                "Skipped backup notification folder open because folder kind '{FolderKindValue}' was invalid.",
                folderKindValue);
            return;
        }

        backupFolderOpener.OpenFolder(folderKind);
    }
}
#endif
