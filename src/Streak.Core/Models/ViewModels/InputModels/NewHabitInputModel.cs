namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class NewHabitInputModel
{
    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }

    // @TODO: Remove this later.
    public CreateHabitDialogResultModel ToResultModel()
    {
        return new CreateHabitDialogResultModel
        {
            Name = Name.Trim(),
            Emoji = string.IsNullOrWhiteSpace(Emoji) ? null : Emoji.Trim()
        };
    }
}
