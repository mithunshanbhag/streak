namespace Streak.Core.UnitTests.Constants;

public sealed class RouteConstantsTests
{
    #region Positive tests

    [Fact]
    public void GetHabitDetails_ShouldReturnConcreteRoute()
    {
        var result = RouteConstants.GetHabitDetails(42);

        result.Should().Be("/habits/42");
    }

    #endregion

    #region Negative tests

    [Fact]
    public void GetHabitDetails_ShouldThrow_WhenHabitIdIsNotPositive()
    {
        var act = () => RouteConstants.GetHabitDetails(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion
}