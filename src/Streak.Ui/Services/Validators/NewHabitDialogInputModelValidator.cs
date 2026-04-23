namespace Streak.Ui.Services.Validators;

public sealed class NewHabitDialogInputModelValidator : AbstractValidator<NewHabitInputModel>
{
    public NewHabitDialogInputModelValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Habit name is required.")
            .Must(BeWithinConfiguredLength)
            .WithMessage($"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.");

        RuleFor(x => x.Emoji)
            .Must(EmojiValidationHelper.IsEmptyOrSingleEmoji)
            .WithMessage("Emoji must be a single emoji.");

        RuleFor(x => x.Description)
            .Must(BeWithinConfiguredDescriptionLength)
            .WithMessage($"Habit description must be {CoreConstants.HabitDescriptionMaxLength} characters or fewer.");
    }

    private static bool BeWithinConfiguredLength(string? name)
    {
        var normalizedName = name?.Trim();

        return normalizedName?.Length is >= CoreConstants.HabitNameMinLength and <= CoreConstants.HabitNameMaxLength;
    }

    private static bool BeWithinConfiguredDescriptionLength(string? description)
    {
        var normalizedDescription = description?.Trim();

        return string.IsNullOrWhiteSpace(normalizedDescription)
               || normalizedDescription.Length <= CoreConstants.HabitDescriptionMaxLength;
    }
}