#if ANDROID
using Android.App;
using Android.Content;
using Application = Android.App.Application;

namespace Streak.Ui.Services.Implementations;

internal static class AndroidBackupNotificationChannelRegistrar
{
    public static void EnsureCreated()
    {
        if (!OperatingSystem.IsAndroidVersionAtLeast(26))
            return;

        var notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager
                                  ?? throw new InvalidOperationException("Android NotificationManager is required to create backup notification channels.");

        if (notificationManager.GetNotificationChannel(BackupNotificationConstants.AndroidChannelId) is not null)
            return;

        var channel = new NotificationChannel(
            BackupNotificationConstants.AndroidChannelId,
            BackupNotificationConstants.AndroidChannelName,
            NotificationImportance.Default)
        {
            Description = BackupNotificationConstants.AndroidChannelDescription
        };

        notificationManager.CreateNotificationChannel(channel);
    }
}
#endif
