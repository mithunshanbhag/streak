using Android.App;
using Android.Content;
using Android.Util;

namespace Streak.Ui.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class AutomatedBackupAlarmReceiver : BroadcastReceiver
{
    private const string LogTag = "StreakAutoBackup";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var isEnabled = AutomatedBackupSettingsStore.GetIsEnabled(SqliteDatabaseBootstrapper.DatabasePath);
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(context, TimeProvider.System, isEnabled);

        if (nextRunUtc is null)
        {
            Log.Info(
                LogTag,
                "Nightly automated backup trigger fired, but automated backups are disabled. Future triggers were cancelled.");
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, TimeZoneInfo.Local);

        Log.Info(
            LogTag,
            $"Nightly automated backup trigger fired. Backup execution is not implemented yet. Next trigger scheduled for {nextRunLocal:O}.");
    }
}
