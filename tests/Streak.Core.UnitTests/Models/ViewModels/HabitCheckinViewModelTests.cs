namespace Streak.Core.UnitTests.Models.ViewModels;

public class HabitCheckinViewModelTests
{
    #region Boundary tests

    [Theory]
    [InlineData(0, null)]
    [InlineData(1, "👍")]
    [InlineData(2, "👍")]
    [InlineData(3, "👏")]
    [InlineData(5, "👏")]
    [InlineData(6, "😎")]
    [InlineData(9, "😎")]
    [InlineData(10, "🔥")]
    [InlineData(29, "🔥")]
    [InlineData(30, "🐐")]
    [InlineData(45, "🐐")]
    public void StreakEmoji_ShouldMatchConfiguredThresholds(int currentStreak, string? expectedEmoji)
    {
        var viewModel = new HabitCheckinViewModel
        {
            HabitId = 1,
            HabitName = "Read",
            HabitEmoji = "📚",
            Streak = currentStreak,
            IsDoneForToday = currentStreak > 0
        };

        viewModel.StreakEmoji.Should().Be(expectedEmoji);
    }

    #endregion

    #region Positive tests

    [Theory]
    [InlineData(0, "0 streak")]
    [InlineData(1, "👍 1 day streak")]
    [InlineData(2, "👍 2 day streak")]
    [InlineData(3, "👏 3 day streak")]
    [InlineData(6, "😎 6 day streak")]
    [InlineData(10, "🔥 10 day streak")]
    [InlineData(30, "🐐 30 day streak")]
    public void StreakText_ShouldShowExpectedDisplayText(int currentStreak, string expectedText)
    {
        var viewModel = new HabitCheckinViewModel
        {
            HabitId = 1,
            HabitName = "Read",
            HabitEmoji = "📚",
            Streak = currentStreak,
            IsDoneForToday = currentStreak > 0
        };

        viewModel.StreakText.Should().Be(expectedText);
    }

    #endregion
}
