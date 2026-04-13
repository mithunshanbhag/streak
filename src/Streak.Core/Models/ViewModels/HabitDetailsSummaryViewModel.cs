namespace Streak.Core.Models.ViewModels;

public sealed class HabitDetailsSummaryViewModel
{
    #region Display Properties

    public required string HabitName { get; set; }

    public string? HabitEmoji { get; set; }

    public string? HabitDescription { get; set; }

    #endregion

    #region Computed Properties

    public string? StreakEmoji => StreakDisplayHelper.GetStreakEmoji(NormalizedStreak);

    public string StreakCountText => NormalizedStreak.ToString(CultureInfo.InvariantCulture);

    public string StreakLabel => NormalizedStreak switch
    {
        <= 0 => "No active streak",
        1 => "day streak",
        _ => "days streak"
    };

    public string StreakSupportText => NormalizedStreak switch
    {
        <= 0 => "Check in today to start your streak.",
        1 => "Checked in for 1 day in a row.",
        _ => $"Checked in for {NormalizedStreak} days in a row."
    };

    public string AppBarTitle => HasHabitEmoji
        ? $"{HabitEmoji} {HabitName}"
        : HabitName;

    #endregion

    #region Hidden Properties

    public required int HabitId { get; set; }

    public required int Streak { get; set; }

    public bool HasHabitEmoji => !string.IsNullOrWhiteSpace(HabitEmoji);

    public bool HasHabitDescription => !string.IsNullOrWhiteSpace(HabitDescription);

    private int NormalizedStreak => StreakDisplayHelper.NormalizeStreak(Streak);

    #endregion
}