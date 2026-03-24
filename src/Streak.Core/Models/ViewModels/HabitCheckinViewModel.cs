namespace Streak.Core.Models.ViewModels;

public sealed class HabitCheckinViewModel
{
    public required int HabitId { get; set; }

    public required string HabitName { get; set; }

    public string? HabitEmoji { get; set; }

    public required int Streak { get; set; }

    public required bool IsDoneForToday { get; set; }

    public string? StreakEmoji => Streak switch
    {
        >= 7 => "🔥",
        >= 1 => "😎",
        _ => null
    };
}