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

        await EnsureHabitExistsAsync(normalizedCheckin.HabitName, cancellationToken);

        var existingCheckin = await _checkinRepository.GetByHabitNameAndDateAsync(
            normalizedCheckin.HabitName,
            normalizedCheckin.CheckinDate,
            cancellationToken);

        if (existingCheckin is null)
        {
            var added = await _checkinRepository.AddAsync(normalizedCheckin, cancellationToken);
            if (!added)
                throw new InvalidOperationException(
                    $"Unable to add checkin for habit '{normalizedCheckin.HabitName}' on '{normalizedCheckin.CheckinDate}'.");

            return normalizedCheckin;
        }

        existingCheckin.IsDone = normalizedCheckin.IsDone;
        existingCheckin.LastUpdatedUtc = normalizedCheckin.LastUpdatedUtc;

        var updated = await _checkinRepository.UpdateAsync(existingCheckin, cancellationToken);
        if (!updated)
            throw new InvalidOperationException(
                $"Unable to update checkin for habit '{normalizedCheckin.HabitName}' on '{normalizedCheckin.CheckinDate}'.");

        return existingCheckin;
    }

    public Task<Checkin> ToggleForTodayAsync(
        string habitName,
        bool isDone,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));

        var checkin = new Checkin
        {
            HabitName = normalizedHabitName,
            CheckinDate = GetUtcTodayDateString(),
            IsDone = isDone ? 1 : 0
        };

        return UpsertAsync(checkin, cancellationToken);
    }

    public async Task DeleteForHabitAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));

        await EnsureHabitExistsAsync(normalizedHabitName, cancellationToken);

        var checkinKey = new CheckinKey(normalizedHabitName, normalizedCheckinDate);
        var checkinExists = await _checkinRepository.ExistsAsync(checkinKey, cancellationToken);
        if (!checkinExists)
            throw new InvalidOperationException(
                $"Checkin for habit '{normalizedHabitName}' on '{normalizedCheckinDate}' does not exist.");

        var deleted = await _checkinRepository.DeleteByHabitNameAndDateAsync(
            normalizedHabitName,
            normalizedCheckinDate,
            cancellationToken);

        if (!deleted)
            throw new InvalidOperationException(
                $"Unable to delete checkin for habit '{normalizedHabitName}' on '{normalizedCheckinDate}'.");
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

    private async Task EnsureHabitExistsAsync(string habitName, CancellationToken cancellationToken)
    {
        var habitExists = await _habitRepository.ExistsByNameAsync(habitName, cancellationToken);
        if (!habitExists) throw new InvalidOperationException($"Habit '{habitName}' does not exist.");
    }

    private static Checkin NormalizeRequiredCheckin(Checkin checkin)
    {
        RequireNotNull(checkin, nameof(checkin));

        var normalizedHabitName = NormalizeRequiredText(checkin.HabitName, nameof(checkin.HabitName));
        var normalizedCheckinDate = NormalizeRequiredDate(checkin.CheckinDate, nameof(checkin.CheckinDate));

        if (checkin.IsDone is not (0 or 1)) throw new ArgumentOutOfRangeException(nameof(checkin.IsDone), "IsDone must be either 0 or 1.");

        return new Checkin
        {
            HabitName = normalizedHabitName,
            CheckinDate = normalizedCheckinDate,
            IsDone = checkin.IsDone,
            LastUpdatedUtc = FormatUtcTimestamp(DateTimeOffset.UtcNow)
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