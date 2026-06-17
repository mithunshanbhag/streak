using Android.App;
using Android.Content;
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
        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var automatedBackupConfigurationService = services.GetRequiredService<IAutomatedBackupConfigurationService>();
        var logger = services.GetRequiredService<ILogger<AutomatedBackupAlarmReceiver>>();
        var timeProvider = services.GetRequiredService<TimeProvider>();
        var localEnabled = automatedBackupConfigurationService.GetIsEnabled();
        var cloudEnabled = automatedBackupConfigurationService.GetIsCloudEnabled();
        var hasAnyEnabled = localEnabled || cloudEnabled;
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(context, timeProvider, hasAnyEnabled);

        Services.AndroidAutomatedBackupWorkEnqueuer.SynchronizeQueuedBackups(
            context,
            localEnabled,
            cloudEnabled);

        if (nextRunUtc is null)
        {
            logger.LogInformation(
                "Nightly automated backup trigger fired via {ExecutionMode}, but automated backups are disabled. Future triggers were cancelled and queued work was cleared.",
                AutomatedBackupConstants.ReceiverExecutionMode);
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, timeProvider.LocalTimeZone);
        logger.LogInformation(
            "Nightly automated backup trigger fired via {ExecutionMode}. Intent action: {IntentAction}. Enqueued local worker: {LocalWorkerEnabled}. Enqueued cloud worker: {CloudWorkerEnabled}. Next trigger scheduled for {NextRunLocal}.",
            AutomatedBackupConstants.ReceiverExecutionMode,
            intentAction,
            localEnabled,
            cloudEnabled,
            nextRunLocal);
    }
}
