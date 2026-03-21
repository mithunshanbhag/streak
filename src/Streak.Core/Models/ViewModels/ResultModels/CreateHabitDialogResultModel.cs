namespace Streak.Core.Models.ViewModels.ResultModels;

public sealed class CreateHabitDialogResultModel
{
    public required string Name { get; init; }

    public string? Emoji { get; init; }
}
