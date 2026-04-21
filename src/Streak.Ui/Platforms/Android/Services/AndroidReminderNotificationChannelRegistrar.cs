using Android.App;
using Android.Content;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

internal static class AndroidReminderNotificationChannelRegistrar
{
    public static void EnsureCreated()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager
                                  ?? throw new InvalidOperationException("Android NotificationManager is required to create reminder notification channels.");

        if (notificationManager.GetNotificationChannel(ReminderNotificationConstants.AndroidChannelId) is not null)
            return;

        var channel = new NotificationChannel(
            ReminderNotificationConstants.AndroidChannelId,
            ReminderNotificationConstants.AndroidChannelName,
            NotificationImportance.Default)
        {
            Description = ReminderNotificationConstants.AndroidChannelDescription
        };

        notificationManager.CreateNotificationChannel(channel);
    }
}
