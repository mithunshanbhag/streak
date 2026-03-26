namespace Streak.Core.Models.ViewModels;

public sealed class HabitCheckinViewModel
{
    #region Display Properties

    public required string HabitName { get; set; }

    public string? HabitEmoji { get; set; }

    public required bool IsDoneForToday { get; set; }

    #endregion

    #region Computed Properties

    public string? StreakEmoji => StreakDisplayHelper.GetStreakEmoji(NormalizedStreak);

    public string StreakText => NormalizedStreak switch
    {
        <= 0 => "0 streak",
        _ => $"{StreakEmoji} {NormalizedStreak} day streak"
    };

    #endregion

    #region Hidden Properties

    public required int HabitId { get; set; }

    public required int Streak { get; set; }

    private int NormalizedStreak => StreakDisplayHelper.NormalizeStreak(Streak);

    #endregion
}