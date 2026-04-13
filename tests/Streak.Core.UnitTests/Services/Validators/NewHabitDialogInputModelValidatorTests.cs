namespace Streak.Core.UnitTests.Services.Validators;

public class NewHabitDialogInputModelValidatorTests
{
    private readonly NewHabitDialogInputModelValidator _sut = new();

    #region Positive tests

    [Fact]
    public void Validate_ShouldPass_WhenNameAndEmojiAreValid()
    {
        var model = new NewHabitInputModel
        {
            Name = "  Read  ",
            Emoji = "  📚  "
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenEmojiIsAFlag()
    {
        var model = new NewHabitInputModel
        {
            Name = "Travel",
            Emoji = "🇺🇸"
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenEmojiIsWindowsEmojiKeyboardFace()
    {
        var model = new NewHabitInputModel
        {
            Name = "Meditate",
            Emoji = "😎"
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionIsNull()
    {
        var model = new NewHabitInputModel
        {
            Name = "Read",
            Description = null
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Negative tests

    [Fact]
    public void Validate_ShouldFail_WhenNameIsMissing()
    {
        var model = new NewHabitInputModel
        {
            Name = "   ",
            Emoji = "📚"
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(NewHabitInputModel.Name) &&
            x.ErrorMessage == "Habit name is required.");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("1")]
    [InlineData("📚📖")]
    [InlineData("ab")]
    public void Validate_ShouldFail_WhenEmojiIsNotASingleEmoji(string emoji)
    {
        var model = new NewHabitInputModel
        {
            Name = "Read",
            Emoji = emoji
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(NewHabitInputModel.Emoji) &&
            x.ErrorMessage == "Emoji must be a single emoji.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenDescriptionExceedsMaximum()
    {
        var model = new NewHabitInputModel
        {
            Name = "Read",
            Description = new string('D', CoreConstants.HabitDescriptionMaxLength + 1)
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(NewHabitInputModel.Description) &&
            x.ErrorMessage == $"Habit description must be {CoreConstants.HabitDescriptionMaxLength} characters or fewer.");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void Validate_ShouldPass_WhenNameLengthMatchesMaximum()
    {
        var model = new NewHabitInputModel
        {
            Name = new string('R', CoreConstants.HabitNameMaxLength),
            Emoji = "1️⃣"
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenNameLengthExceedsMaximum()
    {
        var model = new NewHabitInputModel
        {
            Name = new string('R', CoreConstants.HabitNameMaxLength + 1)
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(x =>
            x.PropertyName == nameof(NewHabitInputModel.Name) &&
            x.ErrorMessage == $"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.");
    }

    [Fact]
    public void Validate_ShouldPass_WhenDescriptionLengthMatchesMaximum()
    {
        var model = new NewHabitInputModel
        {
            Name = "Read",
            Description = new string('D', CoreConstants.HabitDescriptionMaxLength)
        };

        var result = _sut.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    #endregion
}