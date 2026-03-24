namespace Streak.Core.Constants;

public static class RouteConstants
{
    public const string Home = "/";

    public const string HabitDetails = "/habits/{habitId}";

    public const string Settings = "/settings";

    public static string GetHabitDetails(int habitId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(habitId);

        return HabitDetails.Replace("{habitId}", habitId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }
}