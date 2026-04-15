using Streak.Ui.Constants;
using Streak.Ui.Misc.Utilities;

namespace Streak.Ui.UnitTests.Services;

public sealed class AutomatedBackupScheduleCalculatorTests
{
    #region Positive tests

    [Fact]
    public void GetNextRunUtc_ShouldScheduleForToday_WhenLocalTimeIsBefore1130Pm()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var utcNow = new DateTimeOffset(2026, 4, 15, 17, 0, 0, TimeSpan.Zero);

        var result = AutomatedBackupScheduleCalculator.GetNextRunUtc(
            utcNow,
            localTimeZone,
            new TimeOnly(AutomatedBackupConstants.ScheduledHour, AutomatedBackupConstants.ScheduledMinute));

        var resultLocal = TimeZoneInfo.ConvertTime(result, localTimeZone);

        resultLocal.Should().Be(new DateTimeOffset(2026, 4, 15, 23, 30, 0, localTimeZone.BaseUtcOffset));
    }

    [Fact]
    public void GetNextRunUtc_ShouldScheduleImmediatelyForToday_WhenLocalTimeMatches1130PmExactly()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var utcNow = new DateTimeOffset(2026, 4, 15, 18, 0, 0, TimeSpan.Zero);

        var result = AutomatedBackupScheduleCalculator.GetNextRunUtc(
            utcNow,
            localTimeZone,
            new TimeOnly(AutomatedBackupConstants.ScheduledHour, AutomatedBackupConstants.ScheduledMinute));

        var resultLocal = TimeZoneInfo.ConvertTime(result, localTimeZone);

        resultLocal.Should().Be(new DateTimeOffset(2026, 4, 15, 23, 30, 0, localTimeZone.BaseUtcOffset));
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void GetNextRunUtc_ShouldScheduleForTomorrow_WhenLocalTimeIsAfter1130Pm()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var utcNow = new DateTimeOffset(2026, 4, 15, 18, 1, 0, TimeSpan.Zero);

        var result = AutomatedBackupScheduleCalculator.GetNextRunUtc(
            utcNow,
            localTimeZone,
            new TimeOnly(AutomatedBackupConstants.ScheduledHour, AutomatedBackupConstants.ScheduledMinute));

        var resultLocal = TimeZoneInfo.ConvertTime(result, localTimeZone);

        resultLocal.Should().Be(new DateTimeOffset(2026, 4, 16, 23, 30, 0, localTimeZone.BaseUtcOffset));
    }

    #endregion

    #region Negative tests

    [Fact]
    public void GetNextRunUtc_ShouldThrow_WhenTimeProviderIsNull()
    {
        var act = () => AutomatedBackupScheduleCalculator.GetNextRunUtc((TimeProvider)null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("timeProvider");
    }

    #endregion

    #region Private Helper Methods

    private static TimeZoneInfo CreateFixedOffsetTimeZone(int hours, int minutes)
    {
        var offset = new TimeSpan(hours, minutes, 0);
        return TimeZoneInfo.CreateCustomTimeZone(
            id: $"UTC{offset:hh\\:mm}",
            baseUtcOffset: offset,
            displayName: $"UTC{offset:hh\\:mm}",
            standardDisplayName: $"UTC{offset:hh\\:mm}");
    }

    #endregion
}
