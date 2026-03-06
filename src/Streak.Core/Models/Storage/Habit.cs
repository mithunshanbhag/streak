namespace Streak.Core.Models.Storage;

public class Habit
{
    public required int Id { get; set; }

    public required string Name { get; set; }

    public string? Emoji { get; set; }

    public required int DisplayOrder { get; set; }

    public virtual ICollection<Checkin> Checkins { get; set; } = (List<Checkin>)[];
}