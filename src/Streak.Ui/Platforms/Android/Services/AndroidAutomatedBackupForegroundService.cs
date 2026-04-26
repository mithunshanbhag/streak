#if ANDROID
using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Microsoft.Extensions.DependencyInjection;
using Application = Android.App.Application;

namespace Streak.Ui.Platforms.Android.Services;

[Service(Enabled = true, Exported = false)]
public sealed class AndroidAutomatedBackupForegroundService : Service
{
    private CancellationTokenSource? _runCancellationTokenSource;

    public override IBinder? OnBind(Intent? intent)
    {
        return null;
    }

    public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
    {
        AndroidBackupNotificationChannelRegistrar.EnsureCreated();
        StartForeground(
            BackupNotificationConstants.AutomatedBackupForegroundServiceNotificationId,
            CreateRunningNotification());

        _runCancellationTokenSource?.Cancel();
        _runCancellationTokenSource?.Dispose();
        _runCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));

        _ = Task.Run(async () =>
        {
            try
            {
                await AutomatedBackupAlarmReceiver.HandleReceiveAsync(
                    Application.Context,
                    _runCancellationTokenSource.Token);
            }
            catch (System.OperationCanceledException exception)
            {
                var logger = AndroidServiceProviderAccessor
                    .GetRequiredServiceProvider()
                    .GetRequiredService<ILogger<AndroidAutomatedBackupForegroundService>>();
                logger.LogWarning(exception, "Nightly automated backup foreground service timed out.");
            }
            catch (Exception exception)
            {
                var logger = AndroidServiceProviderAccessor
                    .GetRequiredServiceProvider()
                    .GetRequiredService<ILogger<AndroidAutomatedBackupForegroundService>>();
                logger.LogError(exception, "Nightly automated backup foreground service failed.");
            }
            finally
            {
                StopForeground(StopForegroundFlags.Remove);
                StopSelf(startId);
            }
        });

        return StartCommandResult.NotSticky;
    }

    public override void OnDestroy()
    {
        _runCancellationTokenSource?.Cancel();
        _runCancellationTokenSource?.Dispose();
        _runCancellationTokenSource = null;
        base.OnDestroy();
    }

    private static Notification CreateRunningNotification()
    {
        return new NotificationCompat.Builder(Application.Context, BackupNotificationConstants.AndroidChannelId)
            .SetSmallIcon(Resource.Mipmap.appicon)
            .SetContentTitle("Nightly backup running")
            .SetContentText("Uploading your Streak backup.")
            .SetPriority(NotificationCompat.PriorityLow)
            .SetOngoing(true)
            .Build();
    }
}
#endif
