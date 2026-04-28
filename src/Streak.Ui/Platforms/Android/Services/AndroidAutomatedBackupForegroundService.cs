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
        var logger = AndroidLoggerResolver.GetLogger<AndroidAutomatedBackupForegroundService>();
        var intentAction = intent?.Action ?? "(none)";
        logger?.LogInformation(
            "Nightly automated backup foreground service start requested. StartId: {StartId}. Intent action: {IntentAction}.",
            startId,
            intentAction);

        try
        {
            AndroidBackupNotificationChannelRegistrar.EnsureCreated();
            StartForeground(
                BackupNotificationConstants.AutomatedBackupForegroundServiceNotificationId,
                CreateRunningNotification());

            logger?.LogInformation(
                "Nightly automated backup foreground service entered foreground. StartId: {StartId}.",
                startId);

            _runCancellationTokenSource?.Cancel();
            _runCancellationTokenSource?.Dispose();

            var runCancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));
            _runCancellationTokenSource = runCancellationTokenSource;

            _ = Task.Run(async () =>
            {
                var backgroundLogger = AndroidLoggerResolver.GetLogger<AndroidAutomatedBackupForegroundService>();
                backgroundLogger?.LogInformation(
                    "Nightly automated backup foreground service began executing backup pipeline. StartId: {StartId}.",
                    startId);

                try
                {
                    await AutomatedBackupAlarmReceiver.HandleReceiveAsync(
                        Application.Context,
                        runCancellationTokenSource.Token);

                    backgroundLogger?.LogInformation(
                        "Nightly automated backup foreground service backup pipeline completed. StartId: {StartId}.",
                        startId);
                }
                catch (System.OperationCanceledException exception)
                {
                    var resolvedLogger = ResolveLogger();
                    resolvedLogger.LogWarning(exception, "Nightly automated backup foreground service timed out.");
                }
                catch (Exception exception)
                {
                    var resolvedLogger = ResolveLogger();
                    resolvedLogger.LogError(exception, "Nightly automated backup foreground service failed.");
                }
                finally
                {
                    if (ReferenceEquals(_runCancellationTokenSource, runCancellationTokenSource))
                        _runCancellationTokenSource = null;

                    runCancellationTokenSource.Dispose();
                    StopForeground(StopForegroundFlags.Remove);
                    StopSelf(startId);
                }
            });
        }
        catch (Exception exception)
        {
            var resolvedLogger = ResolveLogger();
            resolvedLogger.LogError(
                exception,
                "Nightly automated backup foreground service failed during startup. StartId: {StartId}. Intent action: {IntentAction}.",
                startId,
                intentAction);
            throw;
        }

        return StartCommandResult.NotSticky;
    }

    public override void OnDestroy()
    {
        AndroidLoggerResolver.GetLogger<AndroidAutomatedBackupForegroundService>()
            ?.LogInformation("Nightly automated backup foreground service is being destroyed.");

        _runCancellationTokenSource?.Cancel();
        _runCancellationTokenSource?.Dispose();
        _runCancellationTokenSource = null;
        base.OnDestroy();
    }

    private static ILogger<AndroidAutomatedBackupForegroundService> ResolveLogger()
    {
        return AndroidLoggerResolver.GetLogger<AndroidAutomatedBackupForegroundService>()
               ?? AndroidServiceProviderAccessor
                   .GetRequiredServiceProvider()
                   .GetRequiredService<ILogger<AndroidAutomatedBackupForegroundService>>();
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
