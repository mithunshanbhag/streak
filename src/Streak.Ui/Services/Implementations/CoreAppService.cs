using System.Globalization;
using FluentValidation;
using Streak.Ui.Constants;
using Streak.Ui.Models.ViewModels.InputModels;
using Streak.Ui.Models.ViewModels.ResultModels;
using Streak.Ui.Repositories.Implementations.Sqlite.Entities;
using Streak.Ui.Repositories.Interfaces;
using Streak.Ui.Services.Interfaces;

namespace Streak.Ui.Services.Implementations;

public class CoreAppService(
    ICoreRepository coreRepository,
    IMapper mapper,
    IValidator<HabitCreateInputModel> habitCreateValidator,
    IValidator<HabitUpdateInputModel> habitUpdateValidator,
    IValidator<HabitOrderUpdateInputModel> habitOrderUpdateValidator,
    IValidator<HabitToggleCheckinInputModel> habitToggleCheckinValidator,
    IValidator<HabitTrendQueryInputModel> habitTrendQueryValidator,
    IValidator<ReminderSettingsUpdateInputModel> reminderSettingsValidator) : ICoreAppService
{
    public async Task<IReadOnlyList<HabitViewModel>> GetHabitsAsync(CancellationToken cancellationToken = default)
    {
        var habits = await coreRepository.GetHabitsAsync(cancellationToken);
        return await BuildHabitViewModelsAsync(habits, cancellationToken);
    }

    public async Task<HabitViewModel> CreateHabitAsync(HabitCreateInputModel inputModel, CancellationToken cancellationToken = default)
    {
        await habitCreateValidator.ValidateAndThrowAsync(inputModel, cancellationToken);

        var normalizedName = NormalizeHabitName(inputModel.Name);
        var duplicate = await coreRepository.GetHabitByNameAsync(normalizedName, cancellationToken);
        if (duplicate is not null)
            throw new ValidationException("A habit with this name already exists.");

        var habitCount = await coreRepository.GetHabitCountAsync(cancellationToken);
        if (habitCount >= CoreConstants.MaxHabitCount)
            throw new ValidationException($"You can only have up to {CoreConstants.MaxHabitCount} habits.");

        var existingHabits = await coreRepository.GetHabitsAsync(cancellationToken);
        var nextSortOrder = existingHabits.Count == 0 ? 0 : existingHabits.Max(x => x.SortOrder) + 1;

        var createdHabit = new Habit
        {
            Id = Guid.NewGuid().ToString("N"),
            Name = normalizedName,
            Emoji = NormalizeEmoji(inputModel.Emoji),
            SortOrder = nextSortOrder,
            CreatedAtUtc = DateTime.UtcNow.ToString("O")
        };

        await coreRepository.AddHabitAsync(createdHabit, cancellationToken);
        return await BuildHabitViewModelAsync(createdHabit, cancellationToken);
    }

    public async Task<HabitViewModel> UpdateHabitAsync(HabitUpdateInputModel inputModel, CancellationToken cancellationToken = default)
    {
        await habitUpdateValidator.ValidateAndThrowAsync(inputModel, cancellationToken);

        var existingHabit = await coreRepository.GetHabitByIdAsync(inputModel.HabitId, cancellationToken)
                           ?? throw new KeyNotFoundException($"Habit '{inputModel.HabitId}' was not found.");

        var normalizedName = NormalizeHabitName(inputModel.Name);
        var duplicate = await coreRepository.GetHabitByNameAsync(normalizedName, cancellationToken);
        if (duplicate is not null && duplicate.Id != existingHabit.Id)
            throw new ValidationException("A habit with this name already exists.");

        existingHabit.Name = normalizedName;
        existingHabit.Emoji = NormalizeEmoji(inputModel.Emoji);
        existingHabit.UpdatedAtUtc = DateTime.UtcNow.ToString("O");

        await coreRepository.UpdateHabitAsync(existingHabit, cancellationToken);
        return await BuildHabitViewModelAsync(existingHabit, cancellationToken);
    }

    public async Task DeleteHabitAsync(string habitId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(habitId))
            throw new ValidationException("Habit ID is required.");

        var habit = await coreRepository.GetHabitByIdAsync(habitId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Habit '{habitId}' was not found.");

        await coreRepository.DeleteHabitAsync(habit, cancellationToken);

        var remainingHabits = await coreRepository.GetHabitsAsync(cancellationToken);
        var sortOrder = 0;
        foreach (var remainingHabit in remainingHabits.OrderBy(x => x.SortOrder))
        {
            remainingHabit.SortOrder = sortOrder++;
            remainingHabit.UpdatedAtUtc = DateTime.UtcNow.ToString("O");
        }

        if (remainingHabits.Count > 0)
            await coreRepository.UpdateHabitOrderAsync(remainingHabits, cancellationToken);
    }

    public async Task<IReadOnlyList<HabitViewModel>> UpdateHabitOrderAsync(
        HabitOrderUpdateInputModel inputModel,
        CancellationToken cancellationToken = default)
    {
        await habitOrderUpdateValidator.ValidateAndThrowAsync(inputModel, cancellationToken);

        var habits = await coreRepository.GetHabitsAsync(cancellationToken);
        if (habits.Count != inputModel.HabitIdsInOrder.Count)
            throw new ValidationException("Habit order payload must include all habits.");

        var habitsById = habits.ToDictionary(x => x.Id);
        if (inputModel.HabitIdsInOrder.Any(x => !habitsById.ContainsKey(x)))
            throw new ValidationException("Habit order payload contains unknown habit IDs.");

        for (var index = 0; index < inputModel.HabitIdsInOrder.Count; index++)
        {
            var habit = habitsById[inputModel.HabitIdsInOrder[index]];
            habit.SortOrder = index;
            habit.UpdatedAtUtc = DateTime.UtcNow.ToString("O");
        }

        await coreRepository.UpdateHabitOrderAsync(habits, cancellationToken);
        return await GetHabitsAsync(cancellationToken);
    }

    public async Task<HabitViewModel> ToggleTodayCheckinAsync(
        HabitToggleCheckinInputModel inputModel,
        CancellationToken cancellationToken = default)
    {
        await habitToggleCheckinValidator.ValidateAndThrowAsync(inputModel, cancellationToken);

        var habit = await coreRepository.GetHabitByIdAsync(inputModel.HabitId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Habit '{inputModel.HabitId}' was not found.");

        var today = DateOnly.FromDateTime(DateTime.Today);
        var todayKey = ToCheckinDate(today);
        var existingCheckin = await coreRepository.GetCheckinAsync(habit.Id, todayKey, cancellationToken);

        var isDone = existingCheckin?.IsDone != 1;
        var checkin = existingCheckin ?? new Checkin
        {
            Id = Guid.NewGuid().ToString("N"),
            HabitId = habit.Id,
            CheckinDate = todayKey,
            CreatedAtUtc = DateTime.UtcNow.ToString("O")
        };

        checkin.IsDone = isDone ? 1 : 0;
        checkin.UpdatedAtUtc = DateTime.UtcNow.ToString("O");

        await coreRepository.UpsertCheckinAsync(checkin, cancellationToken);
        return await BuildHabitViewModelAsync(habit, cancellationToken);
    }

    public async Task<HabitTrendViewModel> GetHabitTrendsAsync(
        HabitTrendQueryInputModel inputModel,
        CancellationToken cancellationToken = default)
    {
        await habitTrendQueryValidator.ValidateAndThrowAsync(inputModel, cancellationToken);

        var habit = await coreRepository.GetHabitByIdAsync(inputModel.HabitId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Habit '{inputModel.HabitId}' was not found.");

        var days = Math.Max(inputModel.Days, CoreConstants.MinimumTrendDays);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var fromDate = today.AddDays(-(days - 1));

        var checkins = await coreRepository.GetCheckinsForHabitAsync(
            habit.Id,
            ToCheckinDate(fromDate),
            ToCheckinDate(today),
            cancellationToken);

        var isDoneByDate = checkins
            .Select(x => new { Date = ParseCheckinDate(x.CheckinDate), IsDone = x.IsDone == 1 })
            .Where(x => x.Date.HasValue)
            .GroupBy(x => x.Date!.Value)
            .ToDictionary(x => x.Key, x => x.Last().IsDone);

        var trendDays = new List<HabitTrendDayViewModel>(days);
        var cursor = fromDate;
        while (cursor <= today)
        {
            trendDays.Add(new HabitTrendDayViewModel
            {
                Date = cursor,
                IsDone = isDoneByDate.GetValueOrDefault(cursor)
            });

            cursor = cursor.AddDays(1);
        }

        return new HabitTrendViewModel
        {
            HabitId = habit.Id,
            HabitName = habit.Name,
            HabitEmoji = habit.Emoji,
            CurrentStreak = CalculateCurrentStreak(
                isDoneByDate.Where(x => x.Value).Select(x => x.Key).ToHashSet(),
                today),
            Days = trendDays
        };
    }

    public async Task<ReminderSettingsViewModel> GetReminderSettingsAsync(CancellationToken cancellationToken = default)
    {
        var reminderSettings = await coreRepository.GetReminderSettingsAsync(cancellationToken);
        return mapper.Map<ReminderSettingsViewModel>(reminderSettings);
    }

    public async Task<ReminderSettingsViewModel> UpdateReminderSettingsAsync(
        ReminderSettingsUpdateInputModel inputModel,
        CancellationToken cancellationToken = default)
    {
        await reminderSettingsValidator.ValidateAndThrowAsync(inputModel, cancellationToken);

        var appSettings = await coreRepository.GetReminderSettingsAsync(cancellationToken);
        appSettings.IsReminderEnabled = inputModel.IsReminderEnabled ? 1 : 0;
        appSettings.ReminderTimeLocal = inputModel.ReminderTimeLocal;
        appSettings.UpdatedAtUtc = DateTime.UtcNow;

        await coreRepository.UpdateReminderSettingsAsync(appSettings, cancellationToken);
        return mapper.Map<ReminderSettingsViewModel>(appSettings);
    }

    private async Task<IReadOnlyList<HabitViewModel>> BuildHabitViewModelsAsync(
        IReadOnlyList<Habit> habits,
        CancellationToken cancellationToken)
    {
        if (habits.Count == 0)
            return [];

        var today = DateOnly.FromDateTime(DateTime.Today);
        var checkins = await coreRepository.GetCheckinsForHabitsAsync(
            habits.Select(x => x.Id).ToList(),
            toDateInclusive: ToCheckinDate(today),
            cancellationToken: cancellationToken);

        var checkinsByHabitId = checkins
            .GroupBy(x => x.HabitId)
            .ToDictionary(x => x.Key, x => x.ToList());

        return habits
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .Select(habit =>
            {
                var viewModel = mapper.Map<HabitViewModel>(habit);
                var doneDates = checkinsByHabitId.TryGetValue(habit.Id, out var habitCheckins)
                    ? habitCheckins
                        .Where(x => x.IsDone == 1)
                        .Select(x => ParseCheckinDate(x.CheckinDate))
                        .Where(x => x.HasValue)
                        .Select(x => x!.Value)
                        .ToHashSet()
                    : new HashSet<DateOnly>();

                viewModel.IsCheckedInToday = doneDates.Contains(today);
                viewModel.CurrentStreak = CalculateCurrentStreak(doneDates, today);
                return viewModel;
            })
            .ToList();
    }

    private async Task<HabitViewModel> BuildHabitViewModelAsync(Habit habit, CancellationToken cancellationToken)
    {
        var habits = await BuildHabitViewModelsAsync([habit], cancellationToken);
        return habits.Single();
    }

    private static int CalculateCurrentStreak(IReadOnlySet<DateOnly> doneDates, DateOnly today)
    {
        if (doneDates.Count == 0)
            return 0;

        DateOnly currentDate;
        if (doneDates.Contains(today))
            currentDate = today;
        else
        {
            var yesterday = today.AddDays(-1);
            if (!doneDates.Contains(yesterday))
                return 0;

            currentDate = yesterday;
        }

        var streak = 0;
        while (doneDates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    private static DateOnly? ParseCheckinDate(string checkinDate)
    {
        return DateOnly.TryParseExact(
            checkinDate,
            CoreConstants.CheckinDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var parsedDate)
            ? parsedDate
            : null;
    }

    private static string ToCheckinDate(DateOnly date)
    {
        return date.ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture);
    }

    private static string NormalizeHabitName(string name)
    {
        return name.Trim();
    }

    private static string? NormalizeEmoji(string? emoji)
    {
        return string.IsNullOrWhiteSpace(emoji) ? null : emoji.Trim();
    }
}
