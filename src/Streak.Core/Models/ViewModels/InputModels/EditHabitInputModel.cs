namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class EditHabitInputModel
{
    #region Display Properties

    public required string Name { get; set; }

    public string? Emoji { get; set; }

    #endregion
}