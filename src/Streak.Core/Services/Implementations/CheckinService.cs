namespace Streak.Core.Services.Implementations;

public class CheckinService(
    ICheckinRepository checkinRepository,
    IHabitRepository habitRepository,
    TimeProvider timeProvider)
    : StreakServiceBase, ICheckinService
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly ICheckinRepository _checkinRepository =
        RequireNotNull(checkinRepository, nameof(checkinRepository));

    private readonly IHabitRepository _habitRepository =
        RequireNotNull(habitRepository, nameof(habitRepository));

    private readonly TimeProvider _timeProvider =
        RequireNotNull(timeProvider, nameof(timeProvider));

    public async Task<Checkin?> GetByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));

        return await _checkinRepository.GetByHabitNameAndDateAsync(
            normalizedHabitName,
            normalizedCheckinDate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Checkin>> GetHistoryAsync(
        string habitName,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedFromDate = NormalizeOptionalDate(fromDate, nameof(fromDate));
        var normalizedToDate = NormalizeOptionalDate(toDate, nameof(toDate));

        if (normalizedFromDate is not null &&
            normalizedToDate is not null &&
            string.CompareOrdinal(normalizedFromDate, normalizedToDate) > 0)
            throw new ArgumentException("'fromDate' cannot be greater than 'toDate'.", nameof(fromDate));

        return await _checkinRepository.GetByHabitNamesAsync(
            [normalizedHabitName],
            normalizedFromDate,
            normalizedToDate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<HabitCheckinViewModel>> GetHomePageHabitCheckinsAsync(
        CancellationToken cancellationToken = default)
    {
        var habits = await _habitRepository.GetAllAsync(cancellationToken);
        if (habits.Count == 0) return [];

        var orderedHabits =
            habits.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        var todayLocal = GetTodayLocalDate();
        var checkins = await _checkinRepository.GetByHabitIdsAsync(
            [.. orderedHabits.Select(x => x.Id)],
            toDate: FormatDate(todayLocal),
            cancellationToken: cancellationToken);

        return BuildHomePageHabitCheckinViewModels(orderedHabits, checkins, todayLocal);
    }

    public async Task<Checkin> UpsertAsync(Checkin checkin, CancellationToken cancellationToken = default)
    {
        var normalizedCheckin = NormalizeRequiredCheckin(checkin);

        await EnsureHabitExistsAsync(normalizedCheckin.HabitId, cancellationToken);

        var existingCheckin = await _checkinRepository.GetAsync(
            new CheckinKey(normalizedCheckin.HabitId, normalizedCheckin.CheckinDate),
            cancellationToken);

        if (existingCheckin is null)
        {
            var added = await _checkinRepository.AddAsync(normalizedCheckin, cancellationToken);
            if (!added)
                throw new InvalidOperationException(
                    $"Unable to add checkin for habit id '{normalizedCheckin.HabitId}' on '{normalizedCheckin.CheckinDate}'.");

            return normalizedCheckin;
        }

        return existingCheckin;
    }

    public Task<Checkin?> ToggleForTodayAsync(
        string habitName,
        bool isDone,
        CancellationToken cancellationToken = default)
    {
        return ToggleForTodayInternalAsync(habitName, isDone, cancellationToken);
    }

    public async Task DeleteForHabitAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var habit = await GetRequiredHabitByNameAsync(habitName, cancellationToken);
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));

        var checkinKey = new CheckinKey(habit.Id, normalizedCheckinDate);
        var checkinExists = await _checkinRepository.ExistsAsync(checkinKey, cancellationToken);
        if (!checkinExists)
            throw new InvalidOperationException(
                $"Checkin for habit '{habit.Name}' on '{normalizedCheckinDate}' does not exist.");

        var deleted = await _checkinRepository.DeleteAsync(checkinKey, cancellationToken);

        if (!deleted)
            throw new InvalidOperationException(
                $"Unable to delete checkin for habit '{habit.Name}' on '{normalizedCheckinDate}'.");
    }

    public async Task<int> GetCurrentStreakAsync(string habitName, CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var checkinHistory = await _checkinRepository.GetByHabitNameAsync(normalizedHabitName, cancellationToken);
        return CalculateCurrentStreak(
            BuildCheckinDateSet(checkinHistory),
            GetTodayLocalDate());
    }

    private async Task<Checkin?> ToggleForTodayInternalAsync(
        string habitName,
        bool isDone,
        CancellationToken cancellationToken)
    {
        var habit = await GetRequiredHabitByNameAsync(habitName, cancellationToken);
        var todayDate = GetTodayLocalDateString();

        if (!isDone)
        {
            await _checkinRepository.DeleteAsync(new CheckinKey(habit.Id, todayDate), cancellationToken);
            return null;
        }

        var checkin = new Checkin
        {
            HabitId = habit.Id,
            CheckinDate = todayDate
        };

        return await UpsertAsync(checkin, cancellationToken);
    }

    private async Task<Habit> GetRequiredHabitByNameAsync(string habitName, CancellationToken cancellationToken)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));

        return await _habitRepository.GetByNameAsync(normalizedHabitName, cancellationToken)
               ?? throw new InvalidOperationException($"Habit '{normalizedHabitName}' does not exist.");
    }

    private async Task EnsureHabitExistsAsync(int habitId, CancellationToken cancellationToken)
    {
        ValidateHabitId(habitId, nameof(habitId));

        var habitExists = await _habitRepository.ExistsAsync(habitId, cancellationToken);
        if (!habitExists) throw new InvalidOperationException($"Habit id '{habitId}' does not exist.");
    }

    private static IReadOnlyList<HabitCheckinViewModel> BuildHomePageHabitCheckinViewModels(
        IReadOnlyList<Habit> habits,
        IReadOnlyList<Checkin> checkins,
        DateOnly todayLocal)
    {
        var checkinDatesByHabitId = checkins
            .GroupBy(x => x.HabitId)
            .ToDictionary(
                x => x.Key,
                x => BuildCheckinDateSet(x));

        return
        [
            .. habits.Select(habit =>
            {
                checkinDatesByHabitId.TryGetValue(habit.Id, out var checkinDates);
                return CreateHomePageHabitCheckinViewModel(habit, checkinDates, todayLocal);
            })
        ];
    }

    private static HabitCheckinViewModel CreateHomePageHabitCheckinViewModel(
        Habit habit,
        HashSet<DateOnly>? checkinDates,
        DateOnly todayLocal)
    {
        var safeCheckinDates = checkinDates ?? [];

        return new HabitCheckinViewModel
        {
            HabitId = habit.Id,
            HabitName = habit.Name,
            HabitEmoji = habit.Emoji,
            Streak = CalculateCurrentStreak(safeCheckinDates, todayLocal),
            IsDoneForToday = safeCheckinDates.Contains(todayLocal)
        };
    }

    private static int CalculateCurrentStreak(
        HashSet<DateOnly> checkinDates,
        DateOnly todayLocal)
    {
        if (checkinDates.Count == 0) return 0;

        var streakStartDate = checkinDates.Contains(todayLocal)
            ? todayLocal
            : todayLocal.AddDays(-1);

        var streak = 0;
        var currentDate = streakStartDate;

        while (checkinDates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    private static HashSet<DateOnly> BuildCheckinDateSet(IEnumerable<Checkin> checkins)
    {
        HashSet<DateOnly> checkinDates = [];

        foreach (var checkin in checkins)
        {
            if (!TryParseDate(checkin.CheckinDate, out var checkinDate)) continue;
            checkinDates.Add(checkinDate);
        }

        return checkinDates;
    }

    private static Checkin NormalizeRequiredCheckin(Checkin checkin)
    {
        RequireNotNull(checkin, nameof(checkin));

        var normalizedCheckinDate = NormalizeRequiredDate(checkin.CheckinDate, nameof(checkin.CheckinDate));
        var normalizedNotes = NormalizeOptionalText(checkin.Notes);
        if (checkin.HabitId <= 0)
            throw new ArgumentOutOfRangeException(nameof(checkin.HabitId), "Habit ID must be greater than zero.");
        if (normalizedNotes?.Length > CoreConstants.CheckinNotesMaxLength)
            throw new ArgumentException(
                $"Checkin notes must be {CoreConstants.CheckinNotesMaxLength} characters or fewer.",
                nameof(checkin.Notes));

        return new Checkin
        {
            HabitId = checkin.HabitId,
            CheckinDate = normalizedCheckinDate,
            Notes = normalizedNotes
        };
    }

    private static string NormalizeRequiredDate(string value, string paramName)
    {
        var normalizedDate = NormalizeRequiredText(value, paramName);
        if (!TryParseDate(normalizedDate, out var parsedDate)) throw new ArgumentException($"Date must match format '{DateFormat}'.", paramName);

        return parsedDate.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private static string? NormalizeOptionalDate(string? value, string paramName)
    {
        var normalizedDate = NormalizeOptionalText(value);
        if (normalizedDate is null) return null;

        if (!TryParseDate(normalizedDate, out var parsedDate)) throw new ArgumentException($"Date must match format '{DateFormat}'.", paramName);

        return parsedDate.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private DateOnly GetTodayLocalDate()
    {
        return DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
    }

    private string GetTodayLocalDateString()
    {
        return FormatDate(GetTodayLocalDate());
    }

    private static string FormatDate(DateOnly date)
    {
        return date.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private static void ValidateHabitId(int habitId, string paramName)
    {
        if (habitId <= 0)
            throw new ArgumentOutOfRangeException(paramName, "Habit ID must be greater than zero.");
    }

    private static bool TryParseDate(string value, out DateOnly parsedDate)
    {
        return DateOnly.TryParseExact(
            value,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }
}
