namespace Streak.Core.Models.Storage;

public class Checkin
{
    public string CheckinDate { get; set; } = null!;

    public string HabitName { get; set; } = null!;

    public int IsDone { get; set; }

    public string? LastUpdatedUtc { get; set; }

    public virtual Habit HabitNameNavigation { get; set; } = null!;
}