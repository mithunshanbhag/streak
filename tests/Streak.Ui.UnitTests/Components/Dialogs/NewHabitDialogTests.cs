namespace Streak.Ui.UnitTests.Components.Dialogs;

using Streak.Ui.Components.Dialogs;

public sealed class NewHabitDialogTests : TestContext
{
    public NewHabitDialogTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        Services.AddLogging();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void NewHabitDialog_ShouldCreateHabit_WhenInputIsValid()
    {
        var habitServiceMock = CreateHabitServiceMock(existingHabits: []);
        habitServiceMock
            .Setup(x => x.CreateAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Habit habit, CancellationToken _) => new Habit
            {
                Id = 7,
                Name = habit.Name,
                Emoji = habit.Emoji,
                Description = habit.Description
            });

        RegisterServices(habitServiceMock);
        var dialogProvider = ShowDialog();

        dialogProvider.Find("input").Input("Read");

        dialogProvider.WaitForAssertion(() =>
        {
            var saveButton = dialogProvider.FindAll("button")
                .Single(x => x.TextContent.Contains("SAVE", StringComparison.Ordinal));

            saveButton.HasAttribute("disabled").Should().BeFalse();
        });

        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("SAVE", StringComparison.Ordinal))
            .Click();

        dialogProvider.WaitForAssertion(() =>
        {
            habitServiceMock.Verify(
                x => x.CreateAsync(
                    It.Is<Habit>(habit =>
                        habit.Name == "Read"
                        && string.IsNullOrWhiteSpace(habit.Emoji)
                        && string.IsNullOrWhiteSpace(habit.Description)),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public void NewHabitDialog_ShouldShowDuplicateNameValidation_WhenNameAlreadyExists()
    {
        var habitServiceMock = CreateHabitServiceMock(
        [
            new Habit
            {
                Id = 1,
                Name = "Read"
            }
        ]);

        RegisterServices(habitServiceMock);
        var dialogProvider = ShowDialog();

        dialogProvider.Find("input").Input(" read ");

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("A habit with this name already exists.");

            var saveButton = dialogProvider.FindAll("button")
                .Single(x => x.TextContent.Contains("SAVE", StringComparison.Ordinal));

            saveButton.HasAttribute("disabled").Should().BeTrue();
        });

        habitServiceMock.Verify(
            x => x.CreateAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void NewHabitDialog_ShouldShowEmojiValidation_WhenEmojiIsInvalid()
    {
        var habitServiceMock = CreateHabitServiceMock(existingHabits: []);

        RegisterServices(habitServiceMock);
        var dialogProvider = ShowDialog();

        dialogProvider.Find("input").Input("Read");
        dialogProvider.FindAll("input")[1].Input("ab");

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("Emoji must be a single emoji.");
        });

        habitServiceMock.Verify(
            x => x.CreateAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Private helper methods

    private static Mock<IHabitService> CreateHabitServiceMock(IReadOnlyList<Habit> existingHabits)
    {
        var habitServiceMock = new Mock<IHabitService>();
        habitServiceMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        return habitServiceMock;
    }

    private void RegisterServices(Mock<IHabitService> habitServiceMock)
    {
        Services.AddSingleton(habitServiceMock.Object);
    }

    private IRenderedComponent<MudDialogProvider> ShowDialog()
    {
        var dialogProvider = RenderComponent<MudDialogProvider>();
        Services.GetRequiredService<IDialogService>()
            .ShowAsync<NewHabitDialog>()
            .GetAwaiter()
            .GetResult();

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("New habit"));
        return dialogProvider;
    }

    #endregion
}
