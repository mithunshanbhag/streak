namespace Streak.Core.Models.Storage;

public class Checkin
{
    public required string CheckinDate { get; set; }

    public required string HabitName { get; set; }

    public required int IsDone { get; set; }

    public string? LastUpdatedUtc { get; set; }

    public virtual Habit HabitNameNavigation { get; set; } = null!;
}