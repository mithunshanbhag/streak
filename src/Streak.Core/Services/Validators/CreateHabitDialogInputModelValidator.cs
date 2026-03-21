using FluentValidation;
using Streak.Core.Constants;
using Streak.Core.Models.ViewModels.InputModels;

namespace Streak.Core.Services.Validators;

public sealed class CreateHabitDialogInputModelValidator : AbstractValidator<CreateHabitDialogInputModel>
{
    public CreateHabitDialogInputModelValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("Habit name is required.")
            .Must(BeWithinConfiguredLength)
            .WithMessage($"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.")
            .Must((model, name) => !HasDuplicateName(name, model.ExistingHabitNames))
            .WithMessage("A habit with this name already exists.");

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

    private static bool HasDuplicateName(string? name, IReadOnlyCollection<string>? existingHabitNames)
    {
        var normalizedName = name?.Trim();
        if (string.IsNullOrEmpty(normalizedName) || existingHabitNames is null || existingHabitNames.Count == 0)
            return false;

        return existingHabitNames.Any(existingName =>
            string.Equals(existingName?.Trim(), normalizedName, StringComparison.OrdinalIgnoreCase));
    }
}
