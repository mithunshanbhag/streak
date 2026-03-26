namespace Streak.Core.UnitTests.Misc.Utilities;

public class HabitHistoryHeatmapBuilderTests
{
    #region Positive tests

    [Fact]
    public void Build_ShouldProjectDoneAndMissedCells_AndHighlightToday()
    {
        var today = new DateOnly(2026, 3, 25);
        IReadOnlyList<Checkin> history =
        [
            CreateCheckin(today),
            CreateCheckin(today.AddDays(-2))
        ];

        var result = HabitHistoryHeatmapBuilder.Build(history, today);

        var todayCell = FindCell(result, today);
        var missedYesterdayCell = FindCell(result, today.AddDays(-1));

        todayCell.Should().NotBeNull();
        todayCell!.IsDone.Should().BeTrue();
        todayCell.IsToday.Should().BeTrue();
        todayCell.TooltipText.Should().Be("Mar 25 — Done ✅");

        missedYesterdayCell.Should().NotBeNull();
        missedYesterdayCell!.IsDone.Should().BeFalse();
        missedYesterdayCell.IsToday.Should().BeFalse();
        missedYesterdayCell.TooltipText.Should().Be("Mar 24 — Missed");
    }

    [Fact]
    public void Build_ShouldEmitMonthLabels_WhenVisibleWeeksCrossMonthBoundaries()
    {
        var today = new DateOnly(2026, 3, 25);

        var result = HabitHistoryHeatmapBuilder.Build([], today);

        var monthLabels = result.Weeks
            .Where(x => !string.IsNullOrWhiteSpace(x.MonthLabel))
            .Select(x => x.MonthLabel)
            .ToArray();

        monthLabels.Should().ContainInOrder("Dec", "Jan", "Feb", "Mar");
    }

    #endregion

    #region Negative tests

    [Fact]
    public void Build_ShouldIgnoreInvalidAndFutureCheckinDates()
    {
        var today = new DateOnly(2026, 3, 25);
        IReadOnlyList<Checkin> history =
        [
            new() { HabitId = 1, CheckinDate = "not-a-date" },
            CreateCheckin(today.AddDays(1)),
            CreateCheckin(today)
        ];

        var result = HabitHistoryHeatmapBuilder.Build(history, today);

        FindCell(result, today)!.IsDone.Should().BeTrue();
        FindCell(result, today.AddDays(1)).Should().BeNull();
        result.Weeks
            .SelectMany(x => x.Cells)
            .Count(x => x.IsDone)
            .Should()
            .Be(1);
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void Build_ShouldRenderNinetyVisibleDays_WhenHistoryIsEmpty()
    {
        var today = new DateOnly(2026, 3, 25);

        var result = HabitHistoryHeatmapBuilder.Build([], today);

        var visibleCells = result.Weeks
            .SelectMany(x => x.Cells)
            .Where(x => !x.IsPlaceholder)
            .ToArray();

        visibleCells.Should().HaveCount(CoreConstants.MinimumTrendDays);
        visibleCells.First().Date.Should().Be(today.AddDays(-(CoreConstants.MinimumTrendDays - 1)));
        visibleCells.Last().Date.Should().Be(today);
        result.HasOlderHistory.Should().BeFalse();
        result.DisclosureSummaryText.Should().Be("Review the last 90 days at a glance.");
    }

    [Fact]
    public void Build_ShouldIncludeOlderHistoryOutsideTheInitialWindow()
    {
        var today = new DateOnly(2026, 3, 25);
        var oldestCompletedDate = today.AddDays(-120);
        IReadOnlyList<Checkin> history =
        [
            CreateCheckin(oldestCompletedDate),
            CreateCheckin(today.AddDays(-30)),
            CreateCheckin(today)
        ];

        var result = HabitHistoryHeatmapBuilder.Build(history, today);

        FindCell(result, oldestCompletedDate).Should().NotBeNull();
        result.HasOlderHistory.Should().BeTrue();
        result.DisclosureSummaryText.Should().Be("Showing the latest 90 days first. Scroll left to see older history.");
    }

    [Fact]
    public void Build_ShouldTreatDuplicateCheckinsForTheSameDayAsOneCompletedCell()
    {
        var today = new DateOnly(2026, 3, 25);
        IReadOnlyList<Checkin> history =
        [
            CreateCheckin(today),
            CreateCheckin(today)
        ];

        var result = HabitHistoryHeatmapBuilder.Build(history, today);

        result.Weeks
            .SelectMany(x => x.Cells)
            .Count(x => x.IsDone)
            .Should()
            .Be(1);
    }

    #endregion

    private static Checkin CreateCheckin(DateOnly date)
    {
        return new Checkin
        {
            HabitId = 1,
            CheckinDate = date.ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture)
        };
    }

    private static HabitHistoryHeatmapCellViewModel? FindCell(
        HabitHistoryHeatmapViewModel viewModel,
        DateOnly date)
    {
        return viewModel.Weeks
            .SelectMany(x => x.Cells)
            .SingleOrDefault(x => x.Date == date);
    }
}
