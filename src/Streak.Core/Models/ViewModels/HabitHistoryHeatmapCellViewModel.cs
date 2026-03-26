namespace Streak.Core.Models.ViewModels;

public sealed class HabitHistoryHeatmapCellViewModel
{
    #region Computed Properties

    public string? TooltipText => Date is null
        ? null
        : $"{Date.Value.ToString("MMM d", CultureInfo.InvariantCulture)} — {(IsDone ? "Done ✅" : "Missed")}";

    #endregion

    #region Hidden Properties

    public DateOnly? Date { get; init; }

    public bool IsDone { get; init; }

    public bool IsToday { get; init; }

    public bool IsPlaceholder => Date is null;

    #endregion
}