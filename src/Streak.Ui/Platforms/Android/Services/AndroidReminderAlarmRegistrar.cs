using Android.App;
using Android.Content;

namespace Streak.Ui.Services.Implementations;

internal static class AndroidReminderAlarmRegistrar
{
    public static DateTimeOffset? Synchronize(
        Context context,
        TimeProvider timeProvider,
        bool isEnabled,
        TimeOnly reminderTimeLocal)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager
                           ?? throw new InvalidOperationException("Android AlarmManager is required to schedule daily reminders.");

        var pendingIntent = CreatePendingIntent(context);
        alarmManager.Cancel(pendingIntent);

        if (!isEnabled)
            return null;

        var nextRunUtc = ReminderScheduleCalculator.GetNextRunUtc(timeProvider, reminderTimeLocal);

        if (CanScheduleExactAlarms(alarmManager))
        {
            alarmManager.SetExactAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                nextRunUtc.ToUnixTimeMilliseconds(),
                pendingIntent);
        }
        else
        {
            alarmManager.SetAndAllowWhileIdle(
                AlarmType.RtcWakeup,
                nextRunUtc.ToUnixTimeMilliseconds(),
                pendingIntent);
        }

        return nextRunUtc;
    }

    #region Private Helper Methods

    private static PendingIntent CreatePendingIntent(Context context)
    {
        var intent = new Intent(context, typeof(Streak.Ui.Platforms.Android.ReminderAlarmReceiver));
        intent.SetAction(ReminderConstants.AlarmAction);

        return PendingIntent.GetBroadcast(
                   context,
                   ReminderConstants.AlarmRequestCode,
                   intent,
                   PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)
               ?? throw new InvalidOperationException("Unable to create the reminder alarm pending intent.");
    }

    private static bool CanScheduleExactAlarms(AlarmManager alarmManager)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
            return alarmManager.CanScheduleExactAlarms();

        return true;
    }

    #endregion
}
