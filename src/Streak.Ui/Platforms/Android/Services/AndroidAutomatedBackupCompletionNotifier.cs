#if ANDROID
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidAutomatedBackupCompletionNotifier(
    ILogger<AndroidAutomatedBackupCompletionNotifier> logger)
    : IAutomatedBackupCompletionNotifier
{
    private readonly ILogger<AndroidAutomatedBackupCompletionNotifier> _logger = logger;

    public void NotifyCompleted(SavedFileLocation savedFileLocation)
    {
        ArgumentNullException.ThrowIfNull(savedFileLocation);

        if (!CanPostNotifications())
        {
            _logger.LogInformation(
                "Skipped automated backup completion notification because Android notification permission is unavailable.");
            return;
        }

        AndroidBackupNotificationChannelRegistrar.EnsureCreated();

        var notification = new NotificationCompat.Builder(Application.Context, BackupNotificationConstants.AndroidChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(NotificationTitleText)
            .SetContentText($"Saved to {savedFileLocation.ParentFolderDisplayPath}.")
            .SetStyle(new NotificationCompat.BigTextStyle()
                .BigText($"Nightly backup saved to {savedFileLocation.SavedFileDisplayPath}. Tap to open the backup folder."))
            .SetPriority(NotificationCompat.PriorityDefault)
            .SetAutoCancel(true)
            .SetContentIntent(CreateOpenFolderPendingIntent())
            .Build();

        NotificationManagerCompat
            .From(Application.Context)
            .Notify(BackupNotificationConstants.AutomatedBackupNotificationId, notification);
    }

    private static PendingIntent CreateOpenFolderPendingIntent()
    {
        var intent = new Intent(Application.Context, typeof(Streak.Ui.Platforms.Android.BackupNotificationActionReceiver));
        intent.SetAction(BackupNotificationConstants.OpenFolderAction);
        intent.PutExtra(BackupNotificationConstants.FolderKindExtraKey, BackupFolderKind.AutomatedBackup.ToString());

        return PendingIntent.GetBroadcast(
                   Application.Context,
                   BackupNotificationConstants.AutomatedBackupNotificationId,
                   intent,
                   PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)
               ?? throw new InvalidOperationException("Unable to create the automated backup notification pending intent.");
    }

    private static bool CanPostNotifications()
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(33)
            && ContextCompat.CheckSelfPermission(Application.Context, Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            return false;
        }

        return NotificationManagerCompat.From(Application.Context).AreNotificationsEnabled();
    }

    private const string NotificationTitleText = "Nightly backup complete";
}
#endif
