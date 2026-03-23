namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class NewHabitInputModel
{
    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }

    public IReadOnlyCollection<string> ExistingHabitNames { get; init; } = [];

    public CreateHabitDialogResultModel ToResultModel()
    {
        return new CreateHabitDialogResultModel
        {
            Name = Name.Trim(),
            Emoji = string.IsNullOrWhiteSpace(Emoji) ? null : Emoji.Trim()
        };
    }
}