namespace Streak.Ui.UnitTests.Components.Pages;

using Streak.Core.Models.Storage;
using Streak.Core.Models.ViewModels;
using Streak.Core.Services.Interfaces;

public sealed class HomeTests : TestContext
{
    public HomeTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void Home_ShouldOpenCheckinNoteDialog_WhenUncheckedHabitIsChecked()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("Check in 'Exercise'");
            dialogProvider.Find("input[placeholder='Optional note']");
            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeFalse();
        });

        checkinServiceMock.Verify(
            x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Home_ShouldPersistOptionalNote_WhenCheckinIsConfirmed()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            },
            updatedStreak: 3);

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Find("input[placeholder='Optional note']"));
        dialogProvider.Find("input[placeholder='Optional note']").Input("30 mins cardio");
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Checkin", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync("Exercise", true, "30 mins cardio", It.IsAny<CancellationToken>()),
                Times.Once);

            cut.Markup.Should().Contain("3 day streak");
        });
    }

    [Fact]
    public void Home_ShouldOpenRemoveCheckinDialog_WhenCheckedHabitIsUnchecked()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            },
            updatedStreak: 2);

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?");
            dialogProvider.Markup.Should().Contain("also remove any saved note and picture-proof details");
            dialogProvider.Markup.Should().Contain("Saved note");
            dialogProvider.Markup.Should().Contain("Picture proof");
            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeTrue();
        });

        checkinServiceMock.Verify(
            x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Home_ShouldRemoveCheckin_WhenRemovalIsConfirmed()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            },
            updatedStreak: 2);

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Remove check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync("Exercise", false, null, It.IsAny<CancellationToken>()),
                Times.Once);

            cut.Markup.Should().Contain("2 day streak");
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public void Home_ShouldNotPersistCheckin_WhenNoteDialogIsCancelled()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Check in 'Exercise'"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Cancel", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);

            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeFalse();
            cut.Markup.Should().Contain("2 day streak");
        });
    }

    [Fact]
    public void Home_ShouldNotRemoveCheckin_WhenRemovalDialogIsCancelled()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            },
            updatedStreak: 2);

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Keep check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
                Times.Never);

            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeTrue();
            cut.Markup.Should().Contain("3 day streak");
        });
    }

    [Fact]
    public void Home_ShouldOpenCheckinNoteDialogAgain_WhenRetryingAfterCancelledCheckin()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Check in 'Exercise'"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Cancel", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeFalse());

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Check in 'Exercise'"));
    }

    [Fact]
    public void Home_ShouldOpenRemoveCheckinDialogAgain_WhenRetryingAfterCancelledRemoval()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            });

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Keep check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeTrue());

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void Home_ShouldAllowCheckinWithoutOptionalNote()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            },
            updatedStreak: 3);

        RegisterServices(checkinServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Find("input[placeholder='Optional note']"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Checkin", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync("Exercise", true, string.Empty, It.IsAny<CancellationToken>()),
                Times.Once);

            cut.Markup.Should().Contain("3 day streak");
        });
    }

    #endregion

    #region Private helper methods

    private static Mock<ICheckinService> CreateCheckinServiceMock(
        HabitCheckinViewModel habitCheckinViewModel,
        int updatedStreak = 0)
    {
        var checkinServiceMock = new Mock<ICheckinService>();
        checkinServiceMock
            .Setup(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([habitCheckinViewModel]);
        checkinServiceMock
            .Setup(x => x.GetCurrentStreakAsync(habitCheckinViewModel.HabitName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedStreak);
        checkinServiceMock
            .Setup(x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Checkin?)null);

        return checkinServiceMock;
    }

    private void RegisterServices(Mock<ICheckinService> checkinServiceMock)
    {
        Services.AddSingleton(checkinServiceMock.Object);
    }

    #endregion
}
