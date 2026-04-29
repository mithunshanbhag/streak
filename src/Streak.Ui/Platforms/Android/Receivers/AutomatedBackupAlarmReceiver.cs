using Android.App;
using Android.Content;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class AutomatedBackupAlarmReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var intentAction = intent?.Action ?? "(none)";
        var logger = AndroidLoggerResolver.GetLogger<AutomatedBackupAlarmReceiver>();
        var launchMode = OperatingSystem.IsAndroidVersionAtLeast(26)
            ? "StartForegroundService"
            : "StartService";

        logger?.LogInformation(
            "Nightly automated backup alarm received. Intent action: {IntentAction}. Launch mode: {LaunchMode}. Starting foreground backup service.",
            intentAction,
            launchMode);

        try
        {
            var serviceIntent = new Intent(context, typeof(Services.AndroidAutomatedBackupForegroundService));
            serviceIntent.SetAction(AutomatedBackupConstants.AlarmAction);

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
                context.StartForegroundService(serviceIntent);
            else
                context.StartService(serviceIntent);

            logger?.LogInformation(
                "Nightly automated backup foreground service start request accepted. Intent action: {IntentAction}. Launch mode: {LaunchMode}.",
                intentAction,
                launchMode);
        }
        catch (Exception exception)
        {
            logger?.LogError(
                exception,
                "Nightly automated backup foreground service start request failed. Falling back to receiver execution. Intent action: {IntentAction}. Launch mode: {LaunchMode}.",
                intentAction,
                launchMode);

            StartReceiverFallbackExecution(context, intentAction);
        }
    }

    #region Private Helper Methods

    private void StartReceiverFallbackExecution(Context context, string intentAction)
    {
        var pendingResult = GoAsync()
                            ?? throw new InvalidOperationException("Android broadcast pending result is required for automated backup fallback execution.");

        _ = Task.Run(async () =>
        {
            try
            {
                var logger = AndroidLoggerResolver.GetLogger<AutomatedBackupAlarmReceiver>();
                logger?.LogWarning(
                    "Nightly automated backup receiver fallback execution started after foreground service launch failure. Intent action: {IntentAction}.",
                    intentAction);

                await HandleReceiveAsync(context);

                logger?.LogInformation(
                    "Nightly automated backup receiver fallback execution completed. Intent action: {IntentAction}.",
                    intentAction);
            }
            catch (Exception exception)
            {
                var logger = AndroidServiceProviderAccessor
                    .GetRequiredServiceProvider()
                    .GetRequiredService<ILogger<AutomatedBackupAlarmReceiver>>();
                logger.LogError(exception, "Nightly automated backup receiver fallback execution failed.");
            }
            finally
            {
                pendingResult.Finish();
            }
        });
    }

    internal static async Task HandleReceiveAsync(Context context, CancellationToken cancellationToken = default)
    {
        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var automatedBackupConfigurationService = services.GetRequiredService<IAutomatedBackupConfigurationService>();
        var logger = services.GetRequiredService<ILogger<AutomatedBackupAlarmReceiver>>();
        var timeProvider = services.GetRequiredService<TimeProvider>();
        var automatedBackupCompletionNotifier = services.GetRequiredService<IAutomatedBackupCompletionNotifier>();
        var automatedBackupRunService = services.GetRequiredService<IAutomatedBackupRunService>();

        var isEnabled = automatedBackupConfigurationService.GetHasAnyEnabled();
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(context, timeProvider, isEnabled);

        if (nextRunUtc is null)
        {
            logger.LogInformation(
                "Nightly automated backup trigger fired, but automated backups are disabled. Future triggers were cancelled.");
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, timeProvider.LocalTimeZone);
        var runResult = await automatedBackupRunService.ExecuteEnabledBackupsAsync(cancellationToken);
        if (runResult.HasAnyFailure)
        {
            automatedBackupCompletionNotifier.NotifyFailed(runResult);
        }
        else if (runResult.LocalSavedLocation is not null)
        {
            automatedBackupCompletionNotifier.NotifyCompleted(runResult.LocalSavedLocation);
        }

        logger.LogInformation(
            "Nightly automated backup run completed. Local enabled: {LocalEnabled}. Local succeeded: {LocalSucceeded}. Cloud enabled: {CloudEnabled}. Cloud succeeded: {CloudSucceeded}. Next trigger scheduled for {NextRunLocal}.",
            runResult.LocalEnabled,
            runResult.LocalSucceeded,
            runResult.CloudEnabled,
            runResult.CloudSucceeded,
            nextRunLocal);
    }

    #endregion
}
