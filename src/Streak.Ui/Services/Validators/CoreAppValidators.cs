using FluentValidation;
using Streak.Ui.Constants;
using Streak.Ui.Models.ViewModels.InputModels;

namespace Streak.Ui.Services.Validators;

public class HabitCreateInputModelValidator : AbstractValidator<HabitCreateInputModel>
{
    public HabitCreateInputModelValidator()
    {
        RuleFor(x => x.Name)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Habit name is required.")
            .Must(x =>
            {
                var name = x.Trim();
                return name.Length >= CoreConstants.HabitNameMinLength && name.Length <= CoreConstants.HabitNameMaxLength;
            })
            .WithMessage($"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.");
    }
}

public class HabitUpdateInputModelValidator : AbstractValidator<HabitUpdateInputModel>
{
    public HabitUpdateInputModelValidator()
    {
        RuleFor(x => x.HabitId).NotEmpty();

        RuleFor(x => x.Name)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .WithMessage("Habit name is required.")
            .Must(x =>
            {
                var name = x.Trim();
                return name.Length >= CoreConstants.HabitNameMinLength && name.Length <= CoreConstants.HabitNameMaxLength;
            })
            .WithMessage($"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.");
    }
}

public class HabitOrderUpdateInputModelValidator : AbstractValidator<HabitOrderUpdateInputModel>
{
    public HabitOrderUpdateInputModelValidator()
    {
        RuleFor(x => x.HabitIdsInOrder)
            .NotNull()
            .NotEmpty()
            .Must(x => x.Count <= CoreConstants.MaxHabitCount)
            .WithMessage($"Habit order list cannot exceed {CoreConstants.MaxHabitCount} habits.")
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Habit order list cannot contain duplicate habit IDs.");

        RuleForEach(x => x.HabitIdsInOrder).NotEmpty();
    }
}

public class HabitToggleCheckinInputModelValidator : AbstractValidator<HabitToggleCheckinInputModel>
{
    public HabitToggleCheckinInputModelValidator()
    {
        RuleFor(x => x.HabitId).NotEmpty();
    }
}

public class HabitTrendQueryInputModelValidator : AbstractValidator<HabitTrendQueryInputModel>
{
    public HabitTrendQueryInputModelValidator()
    {
        RuleFor(x => x.HabitId).NotEmpty();
        RuleFor(x => x.Days).GreaterThan(0);
    }
}

public class ReminderSettingsUpdateInputModelValidator : AbstractValidator<ReminderSettingsUpdateInputModel>
{
    public ReminderSettingsUpdateInputModelValidator()
    {
        RuleFor(x => x.ReminderTimeLocal)
            .Must(x => x >= TimeSpan.Zero && x < TimeSpan.FromDays(1))
            .WithMessage("Reminder time must be between 00:00:00 and 23:59:59.");
    }
}
