namespace Streak.Ui.UnitTests.Misc.Utilities;

public class DateDisplayHelperTests
{
    #region Positive tests

    [Fact]
    public void FormatDateBanner_ShouldShowWeekdayMonthAndDayForExplicitCulture()
    {
        var date = new DateTime(2026, 3, 27, 23, 45, 0, DateTimeKind.Local);
        var culture = CultureInfo.GetCultureInfo("en-US");

        var formattedDate = DateDisplayHelper.FormatDateBanner(date, culture);

        formattedDate.Should().Be("Friday, Mar 27");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void FormatDateBanner_ShouldUseCurrentCultureWhenCultureIsNotProvided()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        var date = new DateTime(2026, 3, 27, 0, 5, 0, DateTimeKind.Local);
        var expectedText = date.ToString("dddd, MMM d", CultureInfo.GetCultureInfo("de-DE"));

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");

            var formattedDate = DateDisplayHelper.FormatDateBanner(date);

            formattedDate.Should().Be(expectedText);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    #endregion
}