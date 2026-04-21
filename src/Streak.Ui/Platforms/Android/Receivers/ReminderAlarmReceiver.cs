using Android.App;
using Android.Content;
using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android;

[BroadcastReceiver(Enabled = true, Exported = false)]
public sealed class ReminderAlarmReceiver : BroadcastReceiver
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
                    .GetRequiredService<ILogger<ReminderAlarmReceiver>>();
                logger.LogError(exception, "Daily reminder execution failed.");
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
        var logger = services.GetRequiredService<ILogger<ReminderAlarmReceiver>>();
        var appStoragePathService = services.GetRequiredService<IAppStoragePathService>();
        var timeProvider = services.GetRequiredService<TimeProvider>();
        var checkinService = services.GetRequiredService<ICheckinService>();
        var reminderNotifier = services.GetRequiredService<IReminderNotifier>();

        var settings = ReminderSettingsStore.GetSettings(appStoragePathService.DatabasePath);
        var nextRunUtc = AndroidReminderAlarmRegistrar.Synchronize(
            context,
            timeProvider,
            settings.IsEnabled,
            settings.TimeLocal);

        if (nextRunUtc is null)
        {
            logger.LogInformation(
                "Daily reminder trigger fired, but reminders are disabled. Future triggers were cancelled.");
            return;
        }

        var pendingHabitCount = await checkinService.GetPendingHabitCountForTodayAsync();
        var nextRunLocal = TimeZoneInfo.ConvertTime(nextRunUtc.Value, timeProvider.LocalTimeZone);

        if (pendingHabitCount == 0)
        {
            logger.LogInformation(
                "Skipped daily reminder notification because no habits are pending. Next trigger scheduled for {NextRunLocal}.",
                nextRunLocal);
            return;
        }

        reminderNotifier.NotifyPendingHabits(pendingHabitCount);

        logger.LogInformation(
            "Daily reminder notification posted for {PendingHabitCount} pending habits. Next trigger scheduled for {NextRunLocal}.",
            pendingHabitCount,
            nextRunLocal);
    }

    #endregion
}
