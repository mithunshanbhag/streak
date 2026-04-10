namespace Streak.Core.Constants;

public static class ApiConstants
{
    public const string OwnerHabitsRoute = "v1/owners/{ownerId}/habits";
    public const string OwnerHabitRoute = "v1/owners/{ownerId}/habits/{habitId}";
    public const string OwnerHabitCheckinsRoute = "v1/owners/{ownerId}/habits/{habitId}/checkins";
    public const string OwnerHabitCheckinRoute = "v1/owners/{ownerId}/habits/{habitId}/checkins/{checkinDate}";
    public const string OwnerHabitCheckinTodayRoute = "v1/owners/{ownerId}/habits/{habitId}/checkins/today";
    public const string OwnerHabitStreakRoute = "v1/owners/{ownerId}/habits/{habitId}/streak";

    public static string GetHabitRoute(string ownerId, string habitId)
    {
        return $"v1/owners/{Uri.EscapeDataString(ownerId)}/habits/{Uri.EscapeDataString(habitId)}";
    }
}
