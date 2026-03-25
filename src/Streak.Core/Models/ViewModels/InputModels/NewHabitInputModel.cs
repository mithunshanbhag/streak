namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class NewHabitInputModel
{
    public required string Name { get; set; }

    public string? Emoji { get; set; }
}
