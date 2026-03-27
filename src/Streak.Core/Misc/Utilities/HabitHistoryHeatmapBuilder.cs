namespace Streak.Core.Misc.Utilities;

public static class HabitHistoryHeatmapBuilder
{
    public static HabitHistoryHeatmapViewModel Build(
        IReadOnlyList<Checkin> checkinHistory,
        DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(checkinHistory);

        HashSet<DateOnly> completedDates = [];
        foreach (var checkin in checkinHistory)
        {
            if (!TryParseDate(checkin.CheckinDate, out var parsedDate) || parsedDate > today)
                continue;

            completedDates.Add(parsedDate);
        }

        var ninetyDayWindowStart = today.AddDays(-(CoreConstants.MinimumTrendDays - 1));
        var earliestCompletedDate = completedDates.Count > 0
            ? completedDates.Min()
            : ninetyDayWindowStart;
        var startDate = earliestCompletedDate < ninetyDayWindowStart
            ? earliestCompletedDate
            : ninetyDayWindowStart;

        return new HabitHistoryHeatmapViewModel
        {
            HasOlderHistory = earliestCompletedDate < ninetyDayWindowStart,
            Weeks = BuildWeeks(completedDates, startDate, today)
        };
    }

    private static IReadOnlyList<HabitHistoryHeatmapWeekViewModel> BuildWeeks(
        IReadOnlySet<DateOnly> completedDates,
        DateOnly startDate,
        DateOnly today)
    {
        List<HabitHistoryHeatmapWeekViewModel> weeks = [];
        var firstWeekStart = GetMondayWeekStart(startDate);
        var lastWeekStart = GetMondayWeekStart(today);
        int? previousMonth = null;

        for (var weekStart = firstWeekStart; weekStart <= lastWeekStart; weekStart = weekStart.AddDays(7))
        {
            List<HabitHistoryHeatmapCellViewModel> cells = [];
            DateOnly? firstVisibleDate = null;

            for (var dayOffset = 0; dayOffset < 7; dayOffset++)
            {
                var currentDate = weekStart.AddDays(dayOffset);
                if (currentDate < startDate || currentDate > today)
                {
                    cells.Add(new HabitHistoryHeatmapCellViewModel());
                    continue;
                }

                firstVisibleDate ??= currentDate;

                cells.Add(new HabitHistoryHeatmapCellViewModel
                {
                    Date = currentDate,
                    IsDone = completedDates.Contains(currentDate),
                    IsToday = currentDate == today
                });
            }

            string? monthLabel = null;
            if (firstVisibleDate is not null && previousMonth != firstVisibleDate.Value.Month)
            {
                previousMonth = firstVisibleDate.Value.Month;
                monthLabel = firstVisibleDate.Value.ToString("MMM", CultureInfo.InvariantCulture);
            }

            weeks.Add(new HabitHistoryHeatmapWeekViewModel
            {
                MonthLabel = monthLabel,
                Cells = cells
            });
        }

        return weeks;
    }

    private static DateOnly GetMondayWeekStart(DateOnly date)
    {
        var mondayOffset = ((int)date.DayOfWeek + 6) % 7;
        return date.AddDays(-mondayOffset);
    }

    private static bool TryParseDate(string value, out DateOnly parsedDate)
    {
        return DateOnly.TryParseExact(
            value,
            CoreConstants.CheckinDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }
}