using System.Globalization;
using Streak.Core.Services.Implementations;

namespace Streak.Core.UnitTests.Services;

public class CheckinServiceTests
{
    private static CheckinService CreateSut(
        out Mock<ICheckinRepository> checkinRepositoryMock,
        out Mock<IHabitRepository> habitRepositoryMock)
    {
        checkinRepositoryMock = new Mock<ICheckinRepository>();
        habitRepositoryMock = new Mock<IHabitRepository>();
        return new CheckinService(checkinRepositoryMock.Object, habitRepositoryMock.Object);
    }

    private static Checkin CreateCheckin(string habitName, DateOnly date, int isDone)
    {
        return new Checkin
        {
            HabitName = habitName,
            CheckinDate = ToDateString(date),
            IsDone = isDone
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
            HabitName = "Run",
            CheckinDate = "2025-02-01",
            IsDone = 1
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
            new() { HabitName = "Run", CheckinDate = "2025-01-03", IsDone = 1 },
            new() { HabitName = "Run", CheckinDate = "2025-01-02", IsDone = 0 }
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
    public async Task UpsertAsync_ShouldAddNewCheckin_WhenNoExistingCheckin()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        Checkin? addedCheckin = null;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.ExistsByNameAsync("Run", cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAndDateAsync("Run", "2025-01-10", cancellationToken))
            .ReturnsAsync((Checkin?)null);
        checkinRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Checkin>(), cancellationToken))
            .Callback<Checkin, CancellationToken>((checkin, _) => addedCheckin = checkin)
            .ReturnsAsync(true);

        var result = await sut.UpsertAsync(
            new Checkin
            {
                HabitName = " Run ",
                CheckinDate = " 2025-01-10 ",
                IsDone = 1
            },
            cancellationToken);

        addedCheckin.Should().NotBeNull();
        addedCheckin!.HabitName.Should().Be("Run");
        addedCheckin.CheckinDate.Should().Be("2025-01-10");
        addedCheckin.IsDone.Should().Be(1);

        result.Should().BeSameAs(addedCheckin);
        checkinRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpsertAsync_ShouldUpdateExistingCheckin_WhenCheckinAlreadyExists()
    {
        var cancellationToken = new CancellationTokenSource().Token;
        var existingCheckin = new Checkin
        {
            HabitName = "Run",
            CheckinDate = "2025-01-10",
            IsDone = 0
        };

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.ExistsByNameAsync("Run", cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAndDateAsync("Run", "2025-01-10", cancellationToken))
            .ReturnsAsync(existingCheckin);
        checkinRepositoryMock
            .Setup(x => x.UpdateAsync(existingCheckin, cancellationToken))
            .ReturnsAsync(true);

        var result = await sut.UpsertAsync(
            new Checkin
            {
                HabitName = "Run",
                CheckinDate = "2025-01-10",
                IsDone = 1
            },
            cancellationToken);

        result.Should().BeSameAs(existingCheckin);
        existingCheckin.IsDone.Should().Be(1);
        checkinRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public async Task ToggleForTodayAsync_ShouldUseUtcDateAndMapDoneFlag(bool isDone, int expectedIsDone)
    {
        var beforeUtcDate = ToDateString(DateOnly.FromDateTime(DateTime.UtcNow));
        Checkin? addedCheckin = null;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.ExistsByNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAndDateAsync("Run", It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Checkin?)null);
        checkinRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()))
            .Callback<Checkin, CancellationToken>((checkin, _) => addedCheckin = checkin)
            .ReturnsAsync(true);

        var result = await sut.ToggleForTodayAsync(" Run ", isDone);

        var afterUtcDate = ToDateString(DateOnly.FromDateTime(DateTime.UtcNow));

        addedCheckin.Should().NotBeNull();
        addedCheckin!.HabitName.Should().Be("Run");
        addedCheckin.CheckinDate.Should().MatchRegex("^\\d{4}-\\d{2}-\\d{2}$");
        new[] { beforeUtcDate, afterUtcDate }.Should().Contain(addedCheckin.CheckinDate);
        addedCheckin.IsDone.Should().Be(expectedIsDone);
        result.CheckinDate.Should().Be(addedCheckin.CheckinDate);
        result.IsDone.Should().Be(expectedIsDone);
    }

    [Fact]
    public async Task DeleteForHabitAndDateAsync_ShouldDeleteExistingCheckin()
    {
        var cancellationToken = new CancellationTokenSource().Token;

        var sut = CreateSut(out var checkinRepositoryMock, out var habitRepositoryMock);
        habitRepositoryMock
            .Setup(x => x.ExistsByNameAsync("Run", cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.ExistsAsync(new CheckinKey("Run", "2025-01-07"), cancellationToken))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.DeleteByHabitNameAndDateAsync("Run", "2025-01-07", cancellationToken))
            .ReturnsAsync(true);

        await sut.DeleteForHabitAndDateAsync(" Run ", " 2025-01-07 ", cancellationToken);

        checkinRepositoryMock.Verify(
            x => x.DeleteByHabitNameAndDateAsync("Run", "2025-01-07", cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetCurrentStreakAsync_ShouldReturnConsecutiveDoneDays()
    {
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin("Run", todayUtc, 1),
            CreateCheckin("Run", todayUtc.AddDays(-1), 1),
            CreateCheckin("Run", todayUtc.AddDays(-2), 1),
            CreateCheckin("Run", todayUtc.AddDays(-3), 0)
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
            .Setup(x => x.ExistsByNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        checkinRepositoryMock
            .Setup(x => x.ExistsAsync(new CheckinKey("Run", "2025-01-07"), It.IsAny<CancellationToken>()))
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
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin("Run", todayUtc, 1),
            CreateCheckin("Run", todayUtc.AddDays(-1), 1),
            CreateCheckin("Run", todayUtc.AddDays(-2), 0),
            CreateCheckin("Run", todayUtc.AddDays(-3), 1)
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
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        IReadOnlyList<Checkin> checkinHistory =
        [
            CreateCheckin("Run", todayUtc, 1),
            CreateCheckin("Run", todayUtc.AddDays(-1), 1),
            CreateCheckin("Run", todayUtc.AddDays(-3), 1)
        ];

        var sut = CreateSut(out var checkinRepositoryMock, out _);
        checkinRepositoryMock
            .Setup(x => x.GetByHabitNameAsync("Run", It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkinHistory);

        var result = await sut.GetCurrentStreakAsync("Run");

        result.Should().Be(2);
    }

    #endregion
}
