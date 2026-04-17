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

        var pendingResult = GoAsync();

        _ = Task.Run(async () =>
        {
            try
            {
                await HandleReceiveAsync(context);
            }
            catch (Exception exception)
            {
                var logger = AndroidServiceProviderAccessor
                    .GetRequiredServiceProvider()
                    .GetRequiredService<ILogger<AutomatedBackupAlarmReceiver>>();
                logger.LogError(exception, "Nightly automated backup execution failed.");
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
        var services = AndroidServiceProviderAccessor.GetRequiredServiceProvider();
        var logger = services.GetRequiredService<ILogger<AutomatedBackupAlarmReceiver>>();
        var appStoragePathService = services.GetRequiredService<IAppStoragePathService>();
        var timeProvider = services.GetRequiredService<TimeProvider>();
        var automatedBackupExecutionService = services.GetRequiredService<IAutomatedBackupExecutionService>();

        var isEnabled = AutomatedBackupSettingsStore.GetIsEnabled(appStoragePathService.DatabasePath);
        var nextRunUtc = AndroidAutomatedBackupAlarmRegistrar.Synchronize(context, timeProvider, isEnabled);

        if (nextRunUtc is null)
        {
            logger.LogInformation(
                "Nightly automated backup trigger fired, but automated backups are disabled. Future triggers were cancelled.");
            return;
        }

        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, timeProvider.LocalTimeZone);
        var savedLocation = await automatedBackupExecutionService.ExecuteAutomatedBackupAsync();

        logger.LogInformation(
            "Nightly automated backup completed at {SavedLocation}. Next trigger scheduled for {NextRunLocal}.",
            savedLocation,
            nextRunLocal);
    }

    #endregion
}
