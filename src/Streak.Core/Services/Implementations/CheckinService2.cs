namespace Streak.Core.Services.Implementations;

public sealed class CheckinService2(
    ICheckinRepository2 checkinRepository,
    IHabitRepository2 habitRepository)
    : StreakServiceBase, ICheckinService2
{
    private readonly ICheckinRepository2 _checkinRepository =
        RequireNotNull(checkinRepository, nameof(checkinRepository));

    private readonly IHabitRepository2 _habitRepository =
        RequireNotNull(habitRepository, nameof(habitRepository));

    public async Task<Checkin2?> GetByHabitIdAndDateAsync(
        string habitId,
        string checkinDate,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitId = NormalizeRequiredText(habitId, nameof(habitId));
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));
        var checkin = await _checkinRepository.GetAsync(
            normalizedHabitId,
            normalizedCheckinDate,
            cancellationToken);

        if (checkin is not null || !throwIfNotExists) return checkin;

        throw new KeyNotFoundException(
            $"Checkin for habit '{normalizedHabitId}' on '{normalizedCheckinDate}' was not found.");
    }

    public async Task<IReadOnlyList<Checkin2>> GetHistoryAsync(
        string habitId,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitId = NormalizeRequiredText(habitId, nameof(habitId));
        var normalizedFromDate = NormalizeOptionalDate(fromDate, nameof(fromDate));
        var normalizedToDate = NormalizeOptionalDate(toDate, nameof(toDate));

        if (normalizedFromDate is not null &&
            normalizedToDate is not null &&
            string.CompareOrdinal(normalizedFromDate, normalizedToDate) > 0)
            throw new ArgumentException("'fromDate' cannot be greater than 'toDate'.", nameof(fromDate));

        var queryDefinition = CreateHistoryQueryDefinition(normalizedFromDate, normalizedToDate);
        var checkins = await _checkinRepository.QueryByPartitionAsync(
            normalizedHabitId,
            queryDefinition,
            cancellationToken);

        return [.. checkins];
    }

    public async Task<Checkin2> UpsertAsync(Checkin2 checkin, CancellationToken cancellationToken = default)
    {
        var normalizedCheckin = NormalizeCheckin(checkin, nameof(checkin));
        await EnsureHabitExistsAsync(
            normalizedCheckin.OwnerId,
            normalizedCheckin.HabitId,
            cancellationToken);

        var existingCheckin = await _checkinRepository.GetAsync(
            normalizedCheckin.HabitId,
            normalizedCheckin.Id,
            cancellationToken);

        if (existingCheckin is not null) return existingCheckin;

        await _checkinRepository.AddAsync(normalizedCheckin.HabitId, normalizedCheckin, cancellationToken);
        return normalizedCheckin;
    }

    public async Task<Checkin2?> ToggleForTodayAsync(
        string ownerId,
        string habitId,
        bool isDone,
        CancellationToken cancellationToken = default)
    {
        var habit = await GetRequiredHabitAsync(ownerId, habitId, cancellationToken);
        var todayDate = GetUtcTodayDateString();

        if (!isDone)
        {
            await _checkinRepository.DeleteIfExistsAsync(habit.Id, todayDate, cancellationToken);
            return null;
        }

        var checkin = new Checkin2
        {
            Id = todayDate,
            HabitId = habit.Id,
            CheckinDate = todayDate,
            OwnerId = habit.OwnerId
        };

        return await UpsertAsync(checkin, cancellationToken);
    }

    public async Task DeleteForHabitAndDateAsync(
        string ownerId,
        string habitId,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var habit = await GetRequiredHabitAsync(ownerId, habitId, cancellationToken);
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));
        var deleted = await _checkinRepository.DeleteIfExistsAsync(
            habit.Id,
            normalizedCheckinDate,
            cancellationToken);

        if (!deleted)
            throw new InvalidOperationException(
                $"Checkin for habit '{habit.Id}' on '{normalizedCheckinDate}' does not exist.");
    }

    public async Task<int> GetCurrentStreakAsync(string habitId, CancellationToken cancellationToken = default)
    {
        var normalizedHabitId = NormalizeRequiredText(habitId, nameof(habitId));
        var checkinHistory = await GetHistoryAsync(normalizedHabitId, cancellationToken: cancellationToken);

        if (checkinHistory.Count == 0) return 0;

        HashSet<DateOnly> checkinDates = [];
        foreach (var checkin in checkinHistory)
        {
            if (!TryParseDate(checkin.CheckinDate, out var checkinDate)) continue;
            checkinDates.Add(checkinDate);
        }

        if (checkinDates.Count == 0) return 0;

        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);
        var streakStartDate = checkinDates.Contains(todayUtc)
            ? todayUtc
            : todayUtc.AddDays(-1);

        var streak = 0;
        var currentDate = streakStartDate;

        while (checkinDates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    #region Private Helper Methods

    private async Task<Habit2> GetRequiredHabitAsync(
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        var normalizedOwnerId = NormalizeRequiredText(ownerId, nameof(ownerId));
        var normalizedHabitId = NormalizeRequiredText(habitId, nameof(habitId));

        return await _habitRepository.GetAsync(normalizedOwnerId, normalizedHabitId, cancellationToken)
               ?? throw new InvalidOperationException(
                   $"Habit '{normalizedHabitId}' for owner '{normalizedOwnerId}' does not exist.");
    }

    private async Task EnsureHabitExistsAsync(
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        await GetRequiredHabitAsync(ownerId, habitId, cancellationToken);
    }

    private static QueryDefinition CreateHistoryQueryDefinition(string? fromDate, string? toDate)
    {
        var queryBuilder = new StringBuilder("select * from c");
        var conditions = new List<string>();

        if (fromDate is not null) conditions.Add("c.checkinDate >= @fromDate");
        if (toDate is not null) conditions.Add("c.checkinDate <= @toDate");

        if (conditions.Count > 0)
            queryBuilder
                .Append(" where ")
                .Append(string.Join(" and ", conditions));

        queryBuilder.Append(" order by c.checkinDate desc");

        var queryDefinition = new QueryDefinition(queryBuilder.ToString());
        if (fromDate is not null) queryDefinition.WithParameter("@fromDate", fromDate);
        if (toDate is not null) queryDefinition.WithParameter("@toDate", toDate);

        return queryDefinition;
    }

    private static Checkin2 NormalizeCheckin(Checkin2 checkin, string paramName)
    {
        var nonNullCheckin = RequireNotNull(checkin, paramName);
        var normalizedOwnerId = NormalizeRequiredText(nonNullCheckin.OwnerId, nameof(Checkin2.OwnerId));
        var normalizedHabitId = NormalizeRequiredText(nonNullCheckin.HabitId, nameof(Checkin2.HabitId));
        var normalizedCheckinDate = NormalizeRequiredDate(nonNullCheckin.CheckinDate, nameof(Checkin2.CheckinDate));
        var normalizedId = NormalizeOptionalText(nonNullCheckin.Id);

        if (normalizedId is not null &&
            !string.Equals(normalizedId, normalizedCheckinDate, StringComparison.Ordinal))
            throw new ArgumentException(
                $"Checkin ID must match the normalized check-in date '{normalizedCheckinDate}'.",
                nameof(Checkin2.Id));

        return new Checkin2
        {
            Id = normalizedCheckinDate,
            HabitId = normalizedHabitId,
            CheckinDate = normalizedCheckinDate,
            OwnerId = normalizedOwnerId
        };
    }

    private static string NormalizeRequiredDate(string value, string paramName)
    {
        var normalizedDate = NormalizeRequiredText(value, paramName);
        if (!TryParseDate(normalizedDate, out var parsedDate))
            throw new ArgumentException(
                $"Date must match format '{CoreConstants.CheckinDateFormat}'.",
                paramName);

        return parsedDate.ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture);
    }

    private static string? NormalizeOptionalDate(string? value, string paramName)
    {
        var normalizedDate = NormalizeOptionalText(value);
        if (normalizedDate is null) return null;

        if (!TryParseDate(normalizedDate, out var parsedDate))
            throw new ArgumentException(
                $"Date must match format '{CoreConstants.CheckinDateFormat}'.",
                paramName);

        return parsedDate.ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture);
    }

    private static string GetUtcTodayDateString()
    {
        return DateOnly
            .FromDateTime(DateTime.UtcNow)
            .ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture);
    }

    private static bool TryParseDate(string value, out DateOnly parsedDate)
    {
        return DateOnly.TryParseExact(
            value,
            CoreConstants.CheckinDateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }

    #endregion
}