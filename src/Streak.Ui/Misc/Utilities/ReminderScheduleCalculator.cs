namespace Streak.Ui.Misc.Utilities;

public static class ReminderScheduleCalculator
{
    public static DateTimeOffset GetNextRunUtc(TimeProvider timeProvider, TimeOnly reminderTimeLocal)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        return GetNextRunUtc(
            timeProvider.GetUtcNow(),
            timeProvider.LocalTimeZone,
            reminderTimeLocal);
    }

    public static DateTimeOffset GetNextRunUtc(
        DateTimeOffset utcNow,
        TimeZoneInfo localTimeZone,
        TimeOnly reminderTimeLocal)
    {
        return AutomatedBackupScheduleCalculator.GetNextRunUtc(utcNow, localTimeZone, reminderTimeLocal);
    }
}
