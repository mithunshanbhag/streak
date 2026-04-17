namespace Streak.Core.UnitTests.Services;

public class CheckinServiceTests
{
    private static CheckinService CreateSut(
        out Mock<ICheckinRepository> checkinRepositoryMock,
        out Mock<IHabitRepository> habitRepositoryMock,
        TimeProvider? timeProvider = null)
    {
        checkinRepositoryMock = new Mock<ICheckinRepository>();
        habitRepositoryMock = new Mock<IHabitRepository>();
        return new CheckinService(
            checkinRepositoryMock.Object,
            habitRepositoryMock.Object,
            timeProvider ?? TimeProvider.System);
    }

    private static Checkin CreateCheckin(int habitId, DateOnly date)
    {
        return new Checkin
        {
            HabitId = habitId,
            CheckinDate = ToDateString(date)
        };
    }

    private static string ToDateString(DateOnly date)
    {
        return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    #region Positive tests

    [Fact]
    public async Task GetByHabitNameAndDateAsync_ShouldNormalizeInputs_AndReturnCheckin()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var expectedCheckin = new Checkin
        {
            HabitId = 1,
            CheckinDate = "2025-02-01"
        };

        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAndDateAsync("Run", "2025-02-01", cancellationToken))
            .ReturnsAsync(expectedCheckin);

        var result = await sut.GetByHabitNameAndDateAsync("  Run  ", " 2025-02-01 ", cancellationToken);

        result.Should().BeSameAs(expectedCheckin);
        checkinRepositoryMock.Verify(
            x => x.GetByHabitNameAndDateAsync("Run", "2025-02-01", cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldNormalizeInputs_AndReturnHistoryWithinDateRange()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        IReadOnlyList<Checkin> expectedHistory =
        [
            new() { HabitId = 1, CheckinDate = "2025-01-03" },
            new() { HabitId = 1, CheckinDate = "2025-01-02" }
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNamesAsync(
                It.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Single() == "Run"),
                "2025-01-01",
                "2025-01-31",
                cancellationToken))
            .ReturnsAsync(expectedHistory);

        var result = await sut.GetHistoryAsync(" Run ", " 2025-01-01 ", " 2025-01-31 ", cancellationToken);

        result.Should().BeEquivalentTo(expectedHistory, options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetHomePageHabitCheckinsAsync_ShouldReturnAlphabeticalViewModels_WithTodayStateAndStreaks()
    {
        var indiaTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var localNow = new DateTimeOffset(2026, 4, 14, 1, 0, 0, indiaTimeZone.BaseUtcOffset);
        var timeProvider = new FixedTimeProvider(localNow, indiaTimeZone);
        var cancellationToken = new CancellationTokenSource().Token;
        IReadOnlyList<Habit> habits =
        [
            new() { Id = 2, Name = "Run", Emoji = "🏃" },
            new() { Id = 1, Name = "Read", Emoji = "📚" }
        ];
        IReadOnlyList<Checkin> checkins =
        [
            CreateCheckin(1, new DateOnly(2026, 4, 14)),
            CreateCheckin(1, new DateOnly(2026, 4, 13)),
            CreateCheckin(2, new DateOnly(2026, 4, 13))
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock, timeProvider);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(habits);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitIdsAsync(
                It.Is<IReadOnlyCollection<int>>(ids => ids.Count == 2 && ids.Contains(1) && ids.Contains(2)),
                null,
                "2026-04-14",
                cancellationToken))
            .ReturnsAsync(checkins);

        var result = await sut.GetHomePageHabitCheckinsAsync(cancellationToken);

        result.Select(x => x.HabitName).Should().Equal("Read", "Run");
        result.Select(x => x.IsDoneForToday).Should().Equal(true, false);
        result.Select(x => x.Streak).Should().Equal(2, 1);
    }

    [Fact]
    public async Task UpsertAsync_ShouldAddNewCheckin_WhenNoExistingCheckin()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        Checkin? addedCheckin = null;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.ExistsAsync(1, cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.GetAsync(new CheckinKey(1, "2025-01-10"), cancellationToken))
            .ReturnsAsync((Checkin?)null);
        checkinRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Checkin>(), cancellationToken))
            .Callback<Checkin, CancellationToken>((checkin, _) => addedCheckin = checkin)
            .ReturnsAsync(true);

        var result = await sut.UpsertAsync(
            new Checkin
            {
                HabitId = 1,
                CheckinDate = " 2025-01-10 "
            },
            cancellationToken);

        addedCheckin.Should().NotBeNull();
        addedCheckin!.HabitId.Should().Be(1);
        addedCheckin.CheckinDate.Should().Be("2025-01-10");

        result.Should().BeSameAs(addedCheckin);
        checkinRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpsertAsync_ShouldReturnExistingCheckin_WhenCheckinAlreadyExists()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var existingCheckin = new Checkin
        {
            HabitId = 1,
            CheckinDate = "2025-01-10"
        };

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.ExistsAsync(1, cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.GetAsync(new CheckinKey(1, "2025-01-10"), cancellationToken))
            .ReturnsAsync(existingCheckin);

        var result = await sut.UpsertAsync(
            new Checkin
            {
                HabitId = 1,
                CheckinDate = "2025-01-10"
            },
            cancellationToken);

        result.Should().BeSameAs(existingCheckin);
        checkinRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
        checkinRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ToggleForTodayAsync_ShouldUseLocalDateAndAddCheckin_WhenDone()
    {
        var indiaTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var localNow = new DateTimeOffset(2026, 4, 14, 1, 0, 0, indiaTimeZone.BaseUtcOffset);
        var timeProvider = new FixedTimeProvider(localNow, indiaTimeZone);
        Checkin? addedCheckin = null;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock, timeProvider);
        habitRepositoryMock
            .Setup(x => x.GetByNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Habit { Id = 7, Name = "Run" });
        habitRepositoryMock
            .Setup(x => x.ExistsAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.GetAsync(It.Is<CheckinKey>(key => key.HabitId == 7), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Checkin?)null);
        checkinRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()))
            .Callback<Checkin, CancellationToken>((checkin, _) => addedCheckin = checkin)
            .ReturnsAsync(true);

        var result = await sut.ToggleForTodayAsync(" Run ", true);

        addedCheckin.Should().NotBeNull();
        addedCheckin!.HabitId.Should().Be(7);
        addedCheckin.CheckinDate.Should().Be("2026-04-14");
        result.Should().NotBeNull();
        result.CheckinDate.Should().Be(addedCheckin.CheckinDate);
    }

    [Fact]
    public async Task ToggleForTodayAsync_ShouldUseLocalDateAndDeleteCheckin_WhenNotDone()
    {
        var indiaTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var localNow = new DateTimeOffset(2026, 4, 14, 1, 0, 0, indiaTimeZone.BaseUtcOffset);
        var timeProvider = new FixedTimeProvider(localNow, indiaTimeZone);
        CheckinKey? deletedKey = null;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock, timeProvider);
        habitRepositoryMock
            .Setup(x => x.GetByNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Habit { Id = 7, Name = "Run" });
        checkinRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CheckinKey>(), It.IsAny<CancellationToken>()))
            .Callback<CheckinKey, CancellationToken>((key, _) => deletedKey = key)
            .ReturnsAsync(true);

        var result = await sut.ToggleForTodayAsync(" Run ", false);

        result.Should().BeNull();
        deletedKey.Should().NotBeNull();
        deletedKey!.Value.HabitId.Should().Be(7);
        deletedKey.Value.CheckinDate.Should().Be("2026-04-14");
        checkinRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ToggleForTodayAsync_ShouldReturnNull_WhenUncheckedCheckinDoesNotExist()
    {
        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.GetByNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Habit { Id = 7, Name = "Run" });
        checkinRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<CheckinKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await sut.ToggleForTodayAsync("Run", false);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteForHabitAndDateAsync_ShouldDeleteExistingCheckin()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.GetByNameAsync("Run", cancellationToken))
            .ReturnsAsync(new Habit { Id = 1, Name = "Run" });
        checkinRepositoryMock
            .Setup(x => x.ExistsAsync(new CheckinKey(1, "2025-01-07"), cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.DeleteAsync(new CheckinKey(1, "2025-01-07"), cancellationToken))
            .ReturnsAsync(true);

        await sut.DeleteForHabitAndDateAsync(" Run ", " 2025-01-07 ", cancellationToken);

        checkinRepositoryMock.Verify(
            x => x.DeleteAsync(new CheckinKey(1, "2025-01-07"), cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ShouldReturnConsecutiveDoneDays()
    {
        var todayLocal = DateOnly.FromDateTime(DateTime.Now);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin(1, todayLocal),
            CreateCheckin(1, todayLocal.AddDays(-1)),
            CreateCheckin(1, todayLocal.AddDays(-2))
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkinHistory);

        var result = await sut.GetCurrentStreakAsync(" Run ");

        result.Should().Be(3);
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task GetByHabitNameAndDateAsync_ShouldThrowArgumentException_WhenDateHasInvalidFormat()
    {
        var sut = CreateSut(out _, out _);

        var act = () => sut.GetByHabitNameAndDateAsync("Run", "2025/01/01");

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("checkinDate");
    }

    [Fact]
    public async Task GetHistoryAsync_ShouldThrowArgumentException_WhenFromDateIsAfterToDate()
    {
        var sut = CreateSut(out _, out _);

        var act = () => sut.GetHistoryAsync("Run", "2025-01-31", "2025-01-01");

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("fromDate");
    }

    [Fact]
    public async Task DeleteForHabitAndDateAsync_ShouldThrowInvalidOperationException_WhenCheckinDoesNotExist()
    {
        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.GetByNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Habit { Id = 1, Name = "Run" });
        checkinRepositoryMock
            .Setup(x => x.ExistsAsync(new CheckinKey(1, "2025-01-07"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => sut.DeleteForHabitAndDateAsync("Run", "2025-01-07");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Checkin for habit 'Run' on '2025-01-07' does not exist.");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task GetHistoryAsync_ShouldPassNullDateRange_WhenFromAndToAreWhitespace()
    {
        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNamesAsync(
                It.Is<IReadOnlyCollection<string>>(names => names.Count == 1 && names.Single() == "Run"),
                null,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await sut.GetHistoryAsync(" Run ", " ", " ");

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetHomePageHabitCheckinsAsync_ShouldReturnEmpty_WhenThereAreNoHabits()
    {
        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await sut.GetHomePageHabitCheckinsAsync();

        result.Should().BeEmpty();
        checkinRepositoryMock.Verify(
            x => x.GetByHabitIdsAsync(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ShouldReturnZero_WhenHistoryIsEmpty()
    {
        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await sut.GetCurrentStreakAsync("Run");

        result.Should().Be(0);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ShouldStopAtFirstMissedDay()
    {
        var todayLocal = DateOnly.FromDateTime(DateTime.Now);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin(1, todayLocal.AddDays(-1)),
            CreateCheckin(1, todayLocal.AddDays(-2))
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkinHistory);

        var result = await sut.GetCurrentStreakAsync("Run");

        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ShouldStopWhenDateGapIsEncountered()
    {
        var todayLocal = DateOnly.FromDateTime(DateTime.Now);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin(1, todayLocal),
            CreateCheckin(1, todayLocal.AddDays(-1)),
            CreateCheckin(1, todayLocal.AddDays(-3))
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkinHistory);

        var result = await sut.GetCurrentStreakAsync("Run");

        result.Should().Be(2);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ShouldUseLocalDate_WhenLocalDateHasRolledOverAheadOfUtc()
    {
        var indiaTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var localNow = new DateTimeOffset(2026, 4, 14, 1, 0, 0, indiaTimeZone.BaseUtcOffset);
        var timeProvider = new FixedTimeProvider(localNow, indiaTimeZone);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin(1, new DateOnly(2026, 4, 14))
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out _, timeProvider);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkinHistory);

        var result = await sut.GetCurrentStreakAsync("Run");

        result.Should().Be(1);
    }

    #endregion

    private static TimeZoneInfo CreateFixedOffsetTimeZone(int hours, int minutes)
    {
        var offset = new TimeSpan(hours, minutes, 0);
        return TimeZoneInfo.CreateCustomTimeZone(
            id: $"UTC{offset:hh\\:mm}",
            baseUtcOffset: offset,
            displayName: $"UTC{offset:hh\\:mm}",
            standardDisplayName: $"UTC{offset:hh\\:mm}");
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo localTimeZone) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow.ToUniversalTime();
        }

        public override TimeZoneInfo LocalTimeZone => localTimeZone;
    }
}
