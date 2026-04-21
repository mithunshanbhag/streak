#if ANDROID
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidReminderNotifier(
    ILogger<AndroidReminderNotifier> logger)
    : IReminderNotifier
{
    private readonly ILogger<AndroidReminderNotifier> _logger = logger;

    public void NotifyPendingHabits(int pendingHabitCount)
    {
        if (pendingHabitCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(pendingHabitCount), "Pending habit count must be greater than zero.");

        if (!CanPostNotifications())
        {
            _logger.LogInformation(
                "Skipped reminder notification because Android notification permission is unavailable.");
            return;
        }

        AndroidReminderNotificationChannelRegistrar.EnsureCreated();

        var bodyText = BuildNotificationBodyText(pendingHabitCount);
        var notification = new NotificationCompat.Builder(Application.Context, ReminderNotificationConstants.AndroidChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle(ReminderNotificationConstants.NotificationTitleText)
            .SetContentText(bodyText)
            .SetStyle(new NotificationCompat.BigTextStyle().BigText(bodyText))
            .SetPriority(NotificationCompat.PriorityDefault)
            .SetAutoCancel(true)
            .SetContentIntent(CreateOpenAppPendingIntent())
            .Build();

        NotificationManagerCompat
            .From(Application.Context)
            .Notify(ReminderNotificationConstants.NotificationId, notification);
    }

    #region Private Helper Methods

    private static PendingIntent CreateOpenAppPendingIntent()
    {
        var packageManager = Application.Context.PackageManager
                             ?? throw new InvalidOperationException("Android PackageManager is required to open Streak from a reminder notification.");
        var launchIntent = packageManager.GetLaunchIntentForPackage(Application.Context.PackageName)
                          ?? throw new InvalidOperationException("Unable to create the reminder notification launch intent.");
        launchIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop | ActivityFlags.NewTask);

        return PendingIntent.GetActivity(
                   Application.Context,
                   ReminderNotificationConstants.NotificationId,
                   launchIntent,
                   PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)
               ?? throw new InvalidOperationException("Unable to create the reminder notification pending intent.");
    }

    private static string BuildNotificationBodyText(int pendingHabitCount)
    {
        return $"You have {pendingHabitCount} habit(s) pending today.";
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

    #endregion
}
#endif
