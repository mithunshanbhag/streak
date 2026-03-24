namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class EditHabitInputModel
{
    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }
}
