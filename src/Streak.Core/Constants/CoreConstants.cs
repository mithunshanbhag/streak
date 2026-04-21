namespace Streak.Core.Constants;

public static class CoreConstants
{
    public const int MaxHabitCount = 10;
    public const int HabitNameMinLength = 1;
    public const int HabitNameMaxLength = 30;
    public const int HabitDescriptionMaxLength = 500;
    public const int CheckinNotesMaxLength = 50;
    public const long CheckinProofMaxSizeBytes = 5 * 1024 * 1024;
    public const int MinimumTrendDays = 90;
    public const int ReminderSettingsId = 1;

    public const string CheckinDateFormat = "yyyy-MM-dd";
    public const string CheckinProofModifiedOnFormat = "O";
    public const string DefaultReminderTimeLocal = "21:00:00";
}
