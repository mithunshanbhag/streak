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

        if (!CanPostNotifications("completion"))
            return;

        AndroidBackupNotificationChannelRegistrar.EnsureCreated();

        var context = GetApplicationContext();
        var notificationBuilder = new NotificationCompat.Builder(context, BackupNotificationConstants.AndroidChannelId);
        var bigTextStyle = new NotificationCompat.BigTextStyle();
        bigTextStyle.BigText($"Nightly backup saved to {savedFileLocation.SavedFileDisplayPath}. Tap to open the backup folder.");

        notificationBuilder.SetSmallIcon(Resource.Mipmap.appicon);
        notificationBuilder.SetContentTitle(CompletionNotificationTitleText);
        notificationBuilder.SetContentText($"Saved to {savedFileLocation.ParentFolderDisplayPath}.");
        notificationBuilder.SetStyle(bigTextStyle);
        notificationBuilder.SetPriority(NotificationCompat.PriorityDefault);
        notificationBuilder.SetAutoCancel(true);
        notificationBuilder.SetContentIntent(CreateOpenFolderPendingIntent(context));

        var notification = notificationBuilder.Build()
                           ?? throw new InvalidOperationException("Unable to build the automated backup completion notification.");
        GetNotificationManager(context).Notify(BackupNotificationConstants.AutomatedBackupNotificationId, notification);
    }

    public void NotifyFailed(AutomatedBackupRunResult runResult)
    {
        ArgumentNullException.ThrowIfNull(runResult);

        if (!runResult.HasAnyFailure)
        {
            _logger.LogInformation(
                "Skipped automated backup failure notification because the run result has no failed enabled destinations.");
            return;
        }

        if (!CanPostNotifications("failure"))
            return;

        AndroidBackupNotificationChannelRegistrar.EnsureCreated();

        var (title, body) = GetFailureNotificationText(runResult);
        var context = GetApplicationContext();
        var notificationBuilder = new NotificationCompat.Builder(context, BackupNotificationConstants.AndroidChannelId);
        var bigTextStyle = new NotificationCompat.BigTextStyle();
        bigTextStyle.BigText(body);

        notificationBuilder.SetSmallIcon(Resource.Mipmap.appicon);
        notificationBuilder.SetContentTitle(title);
        notificationBuilder.SetContentText(body);
        notificationBuilder.SetStyle(bigTextStyle);
        notificationBuilder.SetPriority(NotificationCompat.PriorityDefault);
        notificationBuilder.SetAutoCancel(true);
        notificationBuilder.SetContentIntent(CreateOpenAppPendingIntent(context));

        var notification = notificationBuilder.Build()
                           ?? throw new InvalidOperationException("Unable to build the automated backup failure notification.");
        GetNotificationManager(context).Notify(BackupNotificationConstants.AutomatedBackupFailureNotificationId, notification);
    }

    private static (string Title, string Body) GetFailureNotificationText(AutomatedBackupRunResult runResult)
    {
        var localFailed = runResult.LocalEnabled && !runResult.LocalSucceeded;
        var cloudFailed = runResult.CloudEnabled && !runResult.CloudSucceeded;

        if (localFailed && cloudFailed)
        {
            return (
                CombinedFailureNotificationTitleText,
                "Streak could not save any enabled automated backup. Open Settings to check backup options.");
        }

        if (localFailed)
        {
            return (
                LocalFailureNotificationTitleText,
                "Streak could not save the local backup. Open Settings to check backup options.");
        }

        return (
            CloudFailureNotificationTitleText,
            GetCloudFailureNotificationBody(runResult.CloudFailureKind));
    }

    private static string GetCloudFailureNotificationBody(OneDriveBackupFailureKind? failureKind)
    {
        return failureKind switch
        {
            OneDriveBackupFailureKind.NetworkUnavailable =>
                "Streak could not reach OneDrive. It will try again tomorrow.",
            OneDriveBackupFailureKind.AuthRequired =>
                "Reconnect OneDrive to resume cloud backups.",
            OneDriveBackupFailureKind.QuotaExceeded =>
                "OneDrive storage is full. Free up space to resume cloud backups.",
            OneDriveBackupFailureKind.AccessDenied =>
                "Open Settings to check OneDrive backup.",
            _ =>
                "Open Settings to check OneDrive backup."
        };
    }

    private static PendingIntent CreateOpenFolderPendingIntent(Context context)
    {
        var intent = new Intent(context, typeof(Streak.Ui.Platforms.Android.BackupNotificationActionReceiver));
        intent.SetAction(BackupNotificationConstants.OpenFolderAction);
        intent.PutExtra(BackupNotificationConstants.FolderKindExtraKey, BackupFolderKind.AutomatedBackup.ToString());

        return PendingIntent.GetBroadcast(
                   context,
                   BackupNotificationConstants.AutomatedBackupNotificationId,
                   intent,
                   PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)
               ?? throw new InvalidOperationException("Unable to create the automated backup notification pending intent.");
    }

    private static PendingIntent CreateOpenAppPendingIntent(Context context)
    {
        var intent = new Intent(context, typeof(Streak.Ui.Platforms.Android.MainActivity));
        intent.SetAction(BackupNotificationConstants.OpenAppFromFailureAction);
        intent.SetFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);

        return PendingIntent.GetActivity(
                   context,
                   BackupNotificationConstants.AutomatedBackupFailureNotificationId,
                   intent,
                   PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)
               ?? throw new InvalidOperationException("Unable to create the automated backup failure notification pending intent.");
    }

    private bool CanPostNotifications(string notificationKind)
    {
        var context = GetApplicationContext();
        if (OperatingSystem.IsAndroidVersionAtLeast(33)
            && ContextCompat.CheckSelfPermission(context, Manifest.Permission.PostNotifications) != Permission.Granted)
        {
            _logger.LogInformation(
                "Skipped automated backup {NotificationKind} notification because Android notification permission is unavailable.",
                notificationKind);
            return false;
        }

        var notificationsEnabled = GetNotificationManager(context).AreNotificationsEnabled();
        if (!notificationsEnabled)
        {
            _logger.LogInformation(
                "Skipped automated backup {NotificationKind} notification because Android notifications are disabled.",
                notificationKind);
        }

        return notificationsEnabled;
    }

    private static Context GetApplicationContext()
    {
        return Application.Context
               ?? throw new InvalidOperationException("Android application context is required to post automated backup notifications.");
    }

    private static NotificationManagerCompat GetNotificationManager(Context context)
    {
        return NotificationManagerCompat.From(context)
               ?? throw new InvalidOperationException("Android notification manager is required to post automated backup notifications.");
    }

    private const string CompletionNotificationTitleText = "Nightly backup complete";

    private const string LocalFailureNotificationTitleText = "Nightly backup failed";

    private const string CloudFailureNotificationTitleText = "OneDrive backup failed";

    private const string CombinedFailureNotificationTitleText = "Nightly backups failed";
}
#endif
