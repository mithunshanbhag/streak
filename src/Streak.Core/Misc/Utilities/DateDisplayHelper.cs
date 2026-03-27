namespace Streak.Core.Misc.Utilities;

public static class DateDisplayHelper
{
    public static string FormatDateBanner(DateTime date, CultureInfo? culture = null)
    {
        return date.ToString("dddd, MMM d", culture ?? CultureInfo.CurrentCulture);
    }
}