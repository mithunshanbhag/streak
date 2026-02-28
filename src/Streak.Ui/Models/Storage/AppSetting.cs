namespace Streak.Ui.Models.Storage;

public class AppSetting
{
    public int Id { get; set; }

    public int IsReminderEnabled { get; set; }

    public TimeSpan ReminderTimeLocal { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}