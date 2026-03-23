namespace Streak.Core.Models.ViewModels;

public sealed class HabitCheckinCardViewModel
{
    public required int HabitId { get; set; }

    public required string HabitName { get; set; }

    public string? HabitEmoji { get; set; }

    public required int CurrentStreak { get; set; }

    public required bool IsDoneForToday { get; set; }

    public string? StreakEmoji => CurrentStreak switch
    {
        >= 7 => "🔥",
        >= 1 => "😎",
        _ => null
    };
}
