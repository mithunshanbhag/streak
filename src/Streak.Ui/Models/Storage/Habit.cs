namespace Streak.Ui.Models.Storage;

public class Habit
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Emoji { get; set; }

    public int SortOrder { get; set; }

    public string CreatedAtUtc { get; set; } = null!;

    public string? UpdatedAtUtc { get; set; }

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();
}