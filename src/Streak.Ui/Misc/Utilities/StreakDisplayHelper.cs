namespace Streak.Ui.Misc.Utilities;

public static class StreakDisplayHelper
{
    public static int NormalizeStreak(int streak)
    {
        return Math.Max(streak, 0);
    }

    public static string? GetStreakEmoji(int streak)
    {
        return NormalizeStreak(streak) switch
        {
            >= 30 => "🐐",
            >= 10 => "🔥",
            >= 6 => "😎",
            >= 3 => "👏",
            >= 1 => "👍",
            _ => null
        };
    }
}