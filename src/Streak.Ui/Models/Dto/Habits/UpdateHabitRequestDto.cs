namespace Streak.Ui.Models.Dto.Habits;

public sealed class UpdateHabitRequestDto
{
    public required string Name { get; set; }

    public string? Emoji { get; set; }

    public string? Description { get; set; }
}