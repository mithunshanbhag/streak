namespace Streak.Ui.UnitTests.Services;

public sealed class ReminderScheduleCalculatorTests
{
    #region Positive tests

    [Fact]
    public void GetNextRunUtc_ShouldScheduleForToday_WhenLocalTimeIsBefore9Pm()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var utcNow = new DateTimeOffset(2026, 4, 15, 14, 0, 0, TimeSpan.Zero);

        var result = ReminderScheduleCalculator.GetNextRunUtc(
            utcNow,
            localTimeZone,
            new TimeOnly(21, 0));

        var resultLocal = TimeZoneInfo.ConvertTime(result, localTimeZone);

        resultLocal.Should().Be(new DateTimeOffset(2026, 4, 15, 21, 0, 0, localTimeZone.BaseUtcOffset));
    }

    [Fact]
    public void GetNextRunUtc_ShouldScheduleImmediatelyForToday_WhenLocalTimeMatches9PmExactly()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var utcNow = new DateTimeOffset(2026, 4, 15, 15, 30, 0, TimeSpan.Zero);

        var result = ReminderScheduleCalculator.GetNextRunUtc(
            utcNow,
            localTimeZone,
            new TimeOnly(21, 0));

        var resultLocal = TimeZoneInfo.ConvertTime(result, localTimeZone);

        resultLocal.Should().Be(new DateTimeOffset(2026, 4, 15, 21, 0, 0, localTimeZone.BaseUtcOffset));
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void GetNextRunUtc_ShouldScheduleForTomorrow_WhenLocalTimeIsAfter9Pm()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var utcNow = new DateTimeOffset(2026, 4, 15, 15, 31, 0, TimeSpan.Zero);

        var result = ReminderScheduleCalculator.GetNextRunUtc(
            utcNow,
            localTimeZone,
            new TimeOnly(21, 0));

        var resultLocal = TimeZoneInfo.ConvertTime(result, localTimeZone);

        resultLocal.Should().Be(new DateTimeOffset(2026, 4, 16, 21, 0, 0, localTimeZone.BaseUtcOffset));
    }

    #endregion

    #region Negative tests

    [Fact]
    public void GetNextRunUtc_ShouldThrow_WhenTimeProviderIsNull()
    {
        var act = () => ReminderScheduleCalculator.GetNextRunUtc((TimeProvider)null!, new TimeOnly(21, 0));

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
