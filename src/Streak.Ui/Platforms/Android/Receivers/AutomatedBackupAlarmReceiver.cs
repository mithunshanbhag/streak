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

        var logger = AndroidLoggerResolver.GetLogger<AutomatedBackupAlarmReceiver>();
        logger?.LogInformation("Nightly automated backup alarm received; starting foreground backup service.");

        var serviceIntent = new Intent(context, typeof(Services.AndroidAutomatedBackupForegroundService));
        serviceIntent.SetAction(AutomatedBackupConstants.AlarmAction);

        if (OperatingSystem.IsAndroidVersionAtLeast(26))
            context.StartForegroundService(serviceIntent);
        else
            context.StartService(serviceIntent);
    }

    #region Private Helper Methods

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
        if (runResult.LocalSavedLocation is not null)
            automatedBackupCompletionNotifier.NotifyCompleted(runResult.LocalSavedLocation);

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
