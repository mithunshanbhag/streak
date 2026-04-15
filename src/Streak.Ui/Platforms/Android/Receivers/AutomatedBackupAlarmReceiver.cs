using Android.App;
using Android.Content;
using Android.Util;
using Microsoft.Extensions.Logging.Abstractions;

namespace Streak.Ui.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class AutomatedBackupAlarmReceiver : BroadcastReceiver
{
    private const string LogTag = "StreakAutoBackup";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null)
            throw new ArgumentNullException(nameof(context));

        var pendingResult = GoAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await HandleReceiveAsync(context);
            }
            catch (Exception exception)
            {
                Log.Error(LogTag, $"Nightly automated backup execution failed: {exception}");
            }
            finally
            {
                pendingResult.Finish();
            }
        });
    }

    #region Private Helper Methods

    private static async Task HandleReceiveAsync(Context context)
    {
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
        var automatedBackupExecutionService = new AutomatedBackupExecutionService(
            new AppStoragePathService(),
            new AndroidAutomatedBackupFileSaver(),
            NullLogger<AutomatedBackupExecutionService>.Instance);
        var savedLocation = await automatedBackupExecutionService.ExecuteAutomatedBackupAsync();

        Log.Info(
            LogTag,
            $"Nightly automated backup completed at '{savedLocation}'. Next trigger scheduled for {nextRunLocal:O}.");
    }

    #endregion
}
