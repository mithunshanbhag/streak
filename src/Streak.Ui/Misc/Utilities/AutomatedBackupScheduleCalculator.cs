namespace Streak.Ui.Misc.Utilities;

public static class AutomatedBackupScheduleCalculator
{
    public static DateTimeOffset GetNextRunUtc(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        return GetNextRunUtc(
            timeProvider.GetUtcNow(),
            timeProvider.LocalTimeZone,
            new TimeOnly(AutomatedBackupConstants.ScheduledHour, AutomatedBackupConstants.ScheduledMinute));
    }

    public static DateTimeOffset GetNextRunUtc(
        DateTimeOffset utcNow,
        TimeZoneInfo localTimeZone,
        TimeOnly scheduledLocalTime)
    {
        var localNow = TimeZoneInfo.ConvertTime(utcNow, localTimeZone);
        var nextRunDate = localNow.TimeOfDay > scheduledLocalTime.ToTimeSpan()
            ? DateOnly.FromDateTime(localNow.Date.AddDays(1))
            : DateOnly.FromDateTime(localNow.Date);

        var nextRunLocalDateTime = nextRunDate.ToDateTime(scheduledLocalTime);
        var localOffset = localTimeZone.GetUtcOffset(nextRunLocalDateTime);

        return new DateTimeOffset(nextRunLocalDateTime, localOffset).ToUniversalTime();
    }
}
