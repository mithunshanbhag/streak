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
public sealed class ReminderScheduleReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var logger = services.GetRequiredService<ILogger<ReminderScheduleReceiver>>();
        var appStoragePathService = services.GetRequiredService<IAppStoragePathService>();
        var timeProvider = services.GetRequiredService<TimeProvider>();

        var settings = ReminderSettingsStore.GetSettings(appStoragePathService.DatabasePath);
        var nextRunUtc = AndroidReminderAlarmRegistrar.Synchronize(
            context,
            timeProvider,
            settings.IsEnabled,
            settings.TimeLocal);
        var action = intent?.Action ?? "unknown";

        if (nextRunUtc is null)
        {
            logger.LogInformation(
                "Skipped reminder scheduling after {Action} because reminders are disabled.",
                action);
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, timeProvider.LocalTimeZone);
        logger.LogInformation(
            "Rescheduled daily reminder trigger after {Action}. Next trigger scheduled for {NextRunLocal}.",
            action,
            nextRunLocal);
    }
}
