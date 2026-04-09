namespace Streak.Core.Models.Storage;

public class Checkin2
{
    // row key (document id)
    // normalized check-in date (yyyy-MM-dd) within a habit partition.
    public required string Id { get; set; }

    // partition key
    public required string HabitId { get; set; }

    public required string CheckinDate { get; set; }

    // Denormalized owner metadata for tenant-level workflows such as export or account cleanup.
    public required string OwnerId { get; set; }
}