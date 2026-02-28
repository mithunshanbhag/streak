namespace Streak.Ui.Models.ViewModels.ResultModels;

public class HabitViewModel
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Emoji { get; set; }

    public int SortOrder { get; set; }

    public bool IsCheckedInToday { get; set; }

    public int CurrentStreak { get; set; }
}

public class HabitTrendDayViewModel
{
    public DateOnly Date { get; set; }

    public bool IsDone { get; set; }
}

public class HabitTrendViewModel
{
    public string HabitId { get; set; } = string.Empty;

    public string HabitName { get; set; } = string.Empty;

    public string? HabitEmoji { get; set; }

    public int CurrentStreak { get; set; }

    public List<HabitTrendDayViewModel> Days { get; set; } = [];
}

public class ReminderSettingsViewModel
{
    public bool IsReminderEnabled { get; set; }

    public TimeSpan ReminderTimeLocal { get; set; }
}