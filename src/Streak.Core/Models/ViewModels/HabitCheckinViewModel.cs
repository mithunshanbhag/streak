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
        >= 30 => "🐐",
        >= 10 => "🔥",
        >= 6 => "😎",
        >= 3 => "👏",
        _ => null
    };

    public string StreakText => Streak switch
    {
        <= 0 => "0 streak",
        _ when StreakEmoji is null => $"{Streak} day streak",
        _ => $"{StreakEmoji} {Streak} day streak"
    };
}
