namespace Streak.Core.Models.Storage;

public class Owner2
{
    // partition key + row key (document id)
    public required string Id { get; set; }

    public required string DisplayName { get; set; }
}