namespace Streak.Core.Models.ViewModels;

public sealed class HabitDetailsSummaryViewModel
{
    public required int HabitId { get; set; }

    public required string HabitName { get; set; }

    public string? HabitEmoji { get; set; }

    public required int Streak { get; set; }

    private int NormalizedStreak => StreakDisplayHelper.NormalizeStreak(Streak);

    public bool HasHabitEmoji => !string.IsNullOrWhiteSpace(HabitEmoji);

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
}