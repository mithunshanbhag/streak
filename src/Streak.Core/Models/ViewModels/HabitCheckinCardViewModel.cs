namespace Streak.Core.Models.ViewModels;

public class HabitCheckinCardViewModel
{
    public required int HabitId { get; set; }

    public required string HabitName { get; set; }

    public string? HabitEmoji { get; set; }

    public string? StreakEmoji { get; set; }

    public bool IsDoneForToday { get; set; } = false;
}