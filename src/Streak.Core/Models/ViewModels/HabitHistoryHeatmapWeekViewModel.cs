namespace Streak.Core.Models.ViewModels;

public sealed class HabitHistoryHeatmapWeekViewModel
{
    public string? MonthLabel { get; init; }

    public IReadOnlyList<HabitHistoryHeatmapCellViewModel> Cells { get; init; } = [];
}
