using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(
[
    Intent.ActionBootCompleted,
    Intent.ActionMyPackageReplaced,
    Intent.ActionTimeChanged,
    Intent.ActionTimezoneChanged
])]
public sealed class AutomatedBackupScheduleReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var automatedBackupConfigurationService = services.GetRequiredService<IAutomatedBackupConfigurationService>();
        var logger = services.GetRequiredService<ILogger<AutomatedBackupScheduleReceiver>>();
        var timeProvider = services.GetRequiredService<TimeProvider>();

        var isEnabled = automatedBackupConfigurationService.GetHasAnyEnabled();
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(context, timeProvider, isEnabled);
        var action = intent?.Action ?? "unknown";

        if (nextRunUtc is null)
        {
            logger.LogInformation(
                "Skipped nightly automated backup scheduling after {Action} because automated backups are disabled.",
                action);
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, timeProvider.LocalTimeZone);
        logger.LogInformation(
            "Rescheduled nightly automated backup trigger after {Action}. Next trigger scheduled for {NextRunLocal}.",
            action,
            nextRunLocal);
    }
}
