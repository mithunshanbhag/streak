using Android.App;
using Android.Content;
using Android.Util;

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
    private const string LogTag = "StreakAutoBackup";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var isEnabled = AutomatedBackupSettingsStore.GetIsEnabled(SqliteDatabaseBootstrapper.DatabasePath);
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(context, TimeProvider.System, isEnabled);
        var action = intent?.Action ?? "unknown";

        if (nextRunUtc is null)
        {
            Log.Info(
                LogTag,
                $"Skipped nightly automated backup scheduling after '{action}' because automated backups are disabled.");
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, TimeZoneInfo.Local);
        Log.Info(
            LogTag,
            $"Rescheduled nightly automated backup trigger after '{action}'. Next trigger scheduled for {nextRunLocal:O}.");
    }
}
