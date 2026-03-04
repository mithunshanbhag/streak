namespace Streak.Ui.Models.Storage;

public class Habit
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Emoji { get; set; }

    public int DisplayOrder { get; set; }

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();
}