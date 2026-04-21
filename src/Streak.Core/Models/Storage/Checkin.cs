namespace Streak.Core.Models.Storage;

public class Checkin
{
    public required string CheckinDate { get; set; }

    public required int HabitId { get; set; }

    public string? Notes { get; set; }

    public string? ProofImageUri { get; set; }

    public string? ProofImageDisplayName { get; set; }

    public long? ProofImageSizeBytes { get; set; }

    public string? ProofImageModifiedOn { get; set; }

    public virtual Habit HabitNavigation { get; set; } = null!;
}
