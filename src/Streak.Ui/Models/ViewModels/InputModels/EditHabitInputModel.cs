namespace Streak.Ui.Models.ViewModels.InputModels;

public sealed class EditHabitInputModel
{
    #region Display Properties

    public required string Name { get; set; }

    public string? Emoji { get; set; }

    public string? Description { get; set; }

    #endregion
}