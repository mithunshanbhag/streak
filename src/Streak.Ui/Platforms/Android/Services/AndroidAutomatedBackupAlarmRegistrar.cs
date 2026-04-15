using Android.App;
using Android.Content;

namespace Streak.Ui.Services.Implementations;

internal static class AndroidAutomatedBackupAlarmRegistrar
{
    public static DateTimeOffset? Synchronize(Context context, TimeProvider timeProvider, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(timeProvider);

        var alarmManager = context.GetSystemService(Context.AlarmService) as AlarmManager
                           ?? throw new InvalidOperationException("Android AlarmManager is required to schedule nightly automated backups.");

        var pendingIntent = CreatePendingIntent(context);
        alarmManager.Cancel(pendingIntent);

        if (!isEnabled)
            return null;

        var nextRunUtc = AutomatedBackupScheduleCalculator.GetNextRunUtc(timeProvider);

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

    private static PendingIntent CreatePendingIntent(Context context)
    {
        var intent = new Intent(context, typeof(Streak.Ui.Platforms.Android.AutomatedBackupAlarmReceiver));
        intent.SetAction(AutomatedBackupConstants.AlarmAction);

        return PendingIntent.GetBroadcast(
                   context,
                   AutomatedBackupConstants.AlarmRequestCode,
                   intent,
                   PendingIntentFlags.Immutable | PendingIntentFlags.UpdateCurrent)
               ?? throw new InvalidOperationException("Unable to create the automated backup alarm pending intent.");
    }

    private static bool CanScheduleExactAlarms(AlarmManager alarmManager)
    {
        if (OperatingSystem.IsAndroidVersionAtLeast(31))
            return alarmManager.CanScheduleExactAlarms();

        return true;
    }
}
