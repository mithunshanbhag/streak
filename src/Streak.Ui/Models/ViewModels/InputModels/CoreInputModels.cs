namespace Streak.Ui.Models.ViewModels.InputModels;

public class HabitCreateInputModel
{
    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }
}

public class HabitUpdateInputModel
{
    public string HabitId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }
}

public class HabitOrderUpdateInputModel
{
    public List<string> HabitIdsInOrder { get; set; } = [];
}

public class HabitToggleCheckinInputModel
{
    public string HabitId { get; set; } = string.Empty;
}

public class HabitTrendQueryInputModel
{
    public string HabitId { get; set; } = string.Empty;

    public int Days { get; set; } = 90;
}

public class ReminderSettingsUpdateInputModel
{
    public bool IsReminderEnabled { get; set; } = true;

    public TimeSpan ReminderTimeLocal { get; set; } = TimeSpan.Parse("21:00:00");
}