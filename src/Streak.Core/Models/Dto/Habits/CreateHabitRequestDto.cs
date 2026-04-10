namespace Streak.Core.Models.Dto.Habits;

public sealed class CreateHabitRequestDto
{
    public required string Name { get; set; }

    public string? Emoji { get; set; }
}