namespace Streak.Core.UnitTests.Constants;

public sealed class RouteConstantsTests
{
    [Fact]
    public void GetHabitDetails_ShouldReturnConcreteRoute()
    {
        var result = RouteConstants.GetHabitDetails(42);

        result.Should().Be("/habits/42");
    }

    [Fact]
    public void GetHabitDetails_ShouldThrow_WhenHabitIdIsNotPositive()
    {
        var act = () => RouteConstants.GetHabitDetails(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
