namespace Streak.Core.Models.Dto.Habits;

public sealed class HabitResponseDto
{
    public required string Id { get; set; }

    public required string OwnerId { get; set; }

    public required string Name { get; set; }

    public string? Emoji { get; set; }

    public required int CurrentStreak { get; set; }
}
