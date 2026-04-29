namespace Streak.Ui.UnitTests.Components.Pages;

public sealed class HabitDetailsTests : TestContext
{
    public HabitDetailsTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        Services.AddLogging();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void HabitDetails_ShouldRenderLoadedHabitDetails()
    {
        var habitServiceMock = CreateHabitServiceMock(new Habit
        {
            Id = 7,
            Name = "Meditate",
            Emoji = "🧘",
            Description = "Take 10 quiet minutes before bed."
        });
        var checkinServiceMock = CreateCheckinServiceMock("Meditate", streak: 12, history: []);

        RegisterServices(habitServiceMock, checkinServiceMock);
        var cut = RenderHabitDetails(7);

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Meditate");
            cut.Markup.Should().Contain("Take 10 quiet minutes before bed.");
            cut.Markup.Should().Contain("12");
            cut.Markup.Should().Contain("days streak");
            cut.Markup.Should().Contain("Show history");
        });
    }

    [Fact]
    public void HabitDetails_ShouldOpenEditDialog_WhenEditButtonIsClicked()
    {
        var habitServiceMock = CreateHabitServiceMock(new Habit
        {
            Id = 7,
            Name = "Meditate",
            Emoji = "🧘",
            Description = "Take 10 quiet minutes before bed."
        });
        var checkinServiceMock = CreateCheckinServiceMock("Meditate", streak: 12, history: []);

        RegisterServices(habitServiceMock, checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderHabitDetails(7);

        cut.WaitForAssertion(() => cut.Find("button[aria-label='Edit habit']"));
        cut.Find("button[aria-label='Edit habit']").Click();

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("Edit habit");
            dialogProvider.Markup.Should().Contain("Meditate");
            dialogProvider.Markup.Should().Contain("Description (optional)");
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public void HabitDetails_ShouldShowNotFoundState_WhenHabitDoesNotExist()
    {
        var habitServiceMock = CreateHabitServiceMock(null);
        var checkinServiceMock = CreateCheckinServiceMock("Meditate", streak: 0, history: []);

        RegisterServices(habitServiceMock, checkinServiceMock);
        var cut = RenderHabitDetails(7);

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Habit not found.");
            cut.Markup.Should().Contain("Back to habits");
        });
    }

    #endregion

    #region Private helper methods

    private IRenderedComponent<HabitDetails> RenderHabitDetails(int habitId)
    {
        return RenderComponent<HabitDetails>(parameters => parameters.Add(x => x.HabitId, habitId));
    }

    private static Mock<IHabitService> CreateHabitServiceMock(Habit? habit)
    {
        var habitServiceMock = new Mock<IHabitService>();
        habitServiceMock
            .Setup(x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(habit);
        habitServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(habit is null ? [] : [habit]);

        return habitServiceMock;
    }

    private static Mock<ICheckinService> CreateCheckinServiceMock(
        string habitName,
        int streak,
        IReadOnlyList<Checkin> history)
    {
        var checkinServiceMock = new Mock<ICheckinService>();
        checkinServiceMock
            .Setup(x => x.GetCurrentStreakAsync(habitName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(streak);
        checkinServiceMock
            .Setup(x => x.GetHistoryAsync(habitName, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        return checkinServiceMock;
    }

    private void RegisterServices(
        Mock<IHabitService> habitServiceMock,
        Mock<ICheckinService> checkinServiceMock,
        TimeProvider? timeProvider = null)
    {
        Services.AddSingleton(habitServiceMock.Object);
        Services.AddSingleton(checkinServiceMock.Object);
        Services.AddSingleton(timeProvider ?? CreateFixedTimeProvider());
    }

    private static TimeProvider CreateFixedTimeProvider()
    {
        var localTimeZone = CreateFixedOffsetTimeZone(hours: 5, minutes: 30);
        var localNow = new DateTimeOffset(2026, 4, 21, 8, 30, 12, localTimeZone.BaseUtcOffset);

        return new FixedTimeProvider(localNow, localTimeZone);
    }

    private static TimeZoneInfo CreateFixedOffsetTimeZone(int hours, int minutes)
    {
        var offset = new TimeSpan(hours, minutes, 0);
        return TimeZoneInfo.CreateCustomTimeZone(
            id: $"UTC{offset:hh\\:mm}",
            baseUtcOffset: offset,
            displayName: $"UTC{offset:hh\\:mm}",
            standardDisplayName: $"UTC{offset:hh\\:mm}");
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo localTimeZone) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow.ToUniversalTime();
        }

        public override TimeZoneInfo LocalTimeZone => localTimeZone;
    }

    #endregion
}
