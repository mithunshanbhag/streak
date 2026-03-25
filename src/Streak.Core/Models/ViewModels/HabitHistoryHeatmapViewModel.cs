namespace Streak.Core.Models.ViewModels;

public sealed class HabitHistoryHeatmapViewModel
{
    private static readonly IReadOnlyList<string> DefaultDayLabels =
    [
        "M",
        string.Empty,
        "W",
        string.Empty,
        "F",
        string.Empty,
        string.Empty
    ];

    private const string StandardHistorySummaryText = "Review the last 90 days at a glance.";

    private const string ScrollForOlderHistorySummaryText = "Showing the latest 90 days first. Scroll left to see older history.";

    public IReadOnlyList<string> DayLabels { get; init; } = DefaultDayLabels;

    public IReadOnlyList<HabitHistoryHeatmapWeekViewModel> Weeks { get; init; } = [];

    public bool HasOlderHistory { get; init; }

    public string DisclosureSummaryText => HasOlderHistory
        ? ScrollForOlderHistorySummaryText
        : StandardHistorySummaryText;
}
