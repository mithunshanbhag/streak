namespace Streak.Core.Models.Dto.Streaks;

public sealed class CurrentStreakResponseDto
{
    public required string HabitId { get; set; }

    public required int CurrentStreak { get; set; }
}
