namespace Streak.Core.UnitTests.Models.ViewModels;

public class HabitDetailsSummaryViewModelTests
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
        var viewModel = CreateViewModel(currentStreak);

        viewModel.StreakEmoji.Should().Be(expectedEmoji);
    }

    [Theory]
    [InlineData(-4, "0")]
    [InlineData(0, "0")]
    [InlineData(1, "1")]
    [InlineData(12, "12")]
    public void StreakCountText_ShouldClampToZeroOrAbove(int currentStreak, string expectedText)
    {
        var viewModel = CreateViewModel(currentStreak);

        viewModel.StreakCountText.Should().Be(expectedText);
    }

    #endregion

    #region Positive tests

    [Theory]
    [InlineData(0, "No active streak")]
    [InlineData(1, "day streak")]
    [InlineData(2, "days streak")]
    public void StreakLabel_ShouldMatchSummaryDisplay(int currentStreak, string expectedLabel)
    {
        var viewModel = CreateViewModel(currentStreak);

        viewModel.StreakLabel.Should().Be(expectedLabel);
    }

    [Theory]
    [InlineData(0, "Check in today to start your streak.")]
    [InlineData(1, "Checked in for 1 day in a row.")]
    [InlineData(12, "Checked in for 12 days in a row.")]
    public void StreakSupportText_ShouldDescribeSummaryState(int currentStreak, string expectedText)
    {
        var viewModel = CreateViewModel(currentStreak);

        viewModel.StreakSupportText.Should().Be(expectedText);
    }

    [Fact]
    public void AppBarTitle_ShouldIncludeEmoji_WhenOneExists()
    {
        var viewModel = CreateViewModel(12);

        viewModel.AppBarTitle.Should().Be("📚 Read");
    }

    [Fact]
    public void AppBarTitle_ShouldFallbackToHabitName_WhenEmojiIsMissing()
    {
        var viewModel = CreateViewModel(12, habitEmoji: null);

        viewModel.AppBarTitle.Should().Be("Read");
    }

    private static HabitDetailsSummaryViewModel CreateViewModel(int currentStreak, string? habitEmoji = "📚")
    {
        return new HabitDetailsSummaryViewModel
        {
            HabitId = 1,
            HabitName = "Read",
            HabitEmoji = habitEmoji,
            Streak = currentStreak
        };
    }

    #endregion
}
