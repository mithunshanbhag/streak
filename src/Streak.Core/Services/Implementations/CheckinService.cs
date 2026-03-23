using System.Globalization;

namespace Streak.Core.Services.Implementations;

public class CheckinService(
    ICheckinRepository checkinRepository,
    IHabitRepository habitRepository)
    : StreakServiceBase, ICheckinService
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly ICheckinRepository _checkinRepository =
        RequireNotNull(checkinRepository, nameof(checkinRepository));

    private readonly IHabitRepository _habitRepository =
        RequireNotNull(habitRepository, nameof(habitRepository));

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

        existingCheckin.IsDone = normalizedCheckin.IsDone;

        var updated = await _checkinRepository.UpdateAsync(existingCheckin, cancellationToken);
        if (!updated)
            throw new InvalidOperationException(
                $"Unable to update checkin for habit id '{normalizedCheckin.HabitId}' on '{normalizedCheckin.CheckinDate}'.");

        return existingCheckin;
    }

    public Task<Checkin> ToggleForTodayAsync(
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

        if (checkinHistory.Count == 0) return 0;

        var checkinsByDate = new Dictionary<DateOnly, bool>();
        foreach (var checkin in checkinHistory)
        {
            if (!TryParseDate(checkin.CheckinDate, out var checkinDate) || checkinsByDate.ContainsKey(checkinDate)) continue;

            checkinsByDate.Add(checkinDate, checkin.IsDone == 1);
        }

        if (checkinsByDate.Count == 0) return 0;

        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        var streakStartDate = checkinsByDate.TryGetValue(todayUtc, out var isDoneToday) && isDoneToday
            ? todayUtc
            : todayUtc.AddDays(-1);

        var streak = 0;
        var currentDate = streakStartDate;

        while (checkinsByDate.TryGetValue(currentDate, out var isDone) && isDone)
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    private async Task<Checkin> ToggleForTodayInternalAsync(
        string habitName,
        bool isDone,
        CancellationToken cancellationToken)
    {
        var habit = await GetRequiredHabitByNameAsync(habitName, cancellationToken);

        var checkin = new Checkin
        {
            HabitId = habit.Id,
            CheckinDate = GetUtcTodayDateString(),
            IsDone = isDone ? 1 : 0
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
        if (habitId <= 0)
            throw new ArgumentOutOfRangeException(nameof(habitId), "Habit ID must be greater than zero.");

        var habitExists = await _habitRepository.ExistsAsync(habitId, cancellationToken);
        if (!habitExists) throw new InvalidOperationException($"Habit id '{habitId}' does not exist.");
    }

    private static Checkin NormalizeRequiredCheckin(Checkin checkin)
    {
        RequireNotNull(checkin, nameof(checkin));

        var normalizedCheckinDate = NormalizeRequiredDate(checkin.CheckinDate, nameof(checkin.CheckinDate));
        if (checkin.HabitId <= 0)
            throw new ArgumentOutOfRangeException(nameof(checkin.HabitId), "Habit ID must be greater than zero.");

        if (checkin.IsDone is not (0 or 1)) throw new ArgumentOutOfRangeException(nameof(checkin.IsDone), "IsDone must be either 0 or 1.");

        return new Checkin
        {
            HabitId = checkin.HabitId,
            CheckinDate = normalizedCheckinDate,
            IsDone = checkin.IsDone
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

    private static string GetUtcTodayDateString()
    {
        return DateOnly
            .FromDateTime(DateTime.UtcNow)
            .ToString(DateFormat, CultureInfo.InvariantCulture);
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
