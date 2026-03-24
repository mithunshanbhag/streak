namespace Streak.Core.Services.Validators;

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
    }

    private static bool BeWithinConfiguredLength(string? name)
    {
        var normalizedName = name?.Trim();

        return normalizedName is not null
               && normalizedName.Length >= CoreConstants.HabitNameMinLength
               && normalizedName.Length <= CoreConstants.HabitNameMaxLength;
    }
}