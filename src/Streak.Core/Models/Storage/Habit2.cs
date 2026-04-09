namespace Streak.Core.Models.Storage;

public class Habit2
{
    // row key (document id)
    public required string Id { get; set; }

    // partition key
    public required string OwnerId { get; set; }

    public required string Name { get; set; }

    public string? Emoji { get; set; }
}