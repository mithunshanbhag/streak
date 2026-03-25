namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class EditHabitInputModel
{
    public required string Name { get; set; }

    public string? Emoji { get; set; }
}