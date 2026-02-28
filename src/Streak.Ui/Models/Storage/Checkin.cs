namespace Streak.Ui.Models.Storage;

public class Checkin
{
    public string Id { get; set; } = null!;

    public string HabitId { get; set; } = null!;

    public string CheckinDate { get; set; } = null!;

    public int IsDone { get; set; }

    public string CreatedAtUtc { get; set; } = null!;

    public string? UpdatedAtUtc { get; set; }

    public virtual Habit Habit { get; set; } = null!;
}