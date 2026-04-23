namespace Streak.Ui.Models.ViewModels;

public sealed class HabitHistoryHeatmapWeekViewModel
{
    #region Display Properties

    public string? MonthLabel { get; init; }

    public IReadOnlyList<HabitHistoryHeatmapCellViewModel> Cells { get; init; } = [];

    #endregion
}