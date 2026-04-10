namespace Streak.Core.Models.Dto.Checkins;

public sealed class CheckinResponseDto
{
    public required string Id { get; set; }

    public required string HabitId { get; set; }

    public required string OwnerId { get; set; }

    public required string CheckinDate { get; set; }
}
