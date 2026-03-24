namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class NewHabitInputModel
{
    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }
}