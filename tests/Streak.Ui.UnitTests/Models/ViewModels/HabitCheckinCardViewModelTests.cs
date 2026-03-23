using Streak.Core.Models.ViewModels;

namespace Streak.Core.UnitTests.Models.ViewModels;

public class HabitCheckinCardViewModelTests
{
    [Theory]
    [InlineData(0, null)]
    [InlineData(1, "😎")]
    [InlineData(6, "😎")]
    [InlineData(7, "🔥")]
    [InlineData(12, "🔥")]
    public void StreakEmoji_ShouldMatchConfiguredThresholds(int currentStreak, string? expectedEmoji)
    {
        var viewModel = new HabitCheckinCardViewModel
        {
            HabitId = 1,
            HabitName = "Read",
            HabitEmoji = "📚",
            CurrentStreak = currentStreak,
            IsDoneForToday = currentStreak > 0
        };

        viewModel.StreakEmoji.Should().Be(expectedEmoji);
    }
}
