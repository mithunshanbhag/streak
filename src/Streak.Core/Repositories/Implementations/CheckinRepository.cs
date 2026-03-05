namespace Streak.Core.Repositories.Implementations;

public class CheckinRepository(StreakDbContext dbContext) : SqlGenericRepositoryBase<Checkin, CheckinKey>(dbContext), ICheckinRepository
{
    public async Task<IReadOnlyList<Checkin>> GetByHabitNameAsync(string habitName, CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));

        return await Query()
            .Where(x => x.HabitName == normalizedHabitName)
            .OrderByDescending(x => x.CheckinDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Checkin>> GetByHabitNamesAsync(
        IReadOnlyCollection<string> habitNames,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(habitNames);

        if (habitNames.Count == 0)
        {
            return [];
        }

        var normalizedHabitNames = habitNames
            .Select(x => NormalizeRequiredText(x, nameof(habitNames)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var query = Query().Where(x => normalizedHabitNames.Contains(x.HabitName));

        query = ApplyDateRange(query, fromDate, toDate);

        return await query
            .OrderBy(x => x.HabitName)
            .ThenByDescending(x => x.CheckinDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Checkin?> GetByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredText(checkinDate, nameof(checkinDate));

        return await Query().SingleOrDefaultAsync(
            x => x.HabitName == normalizedHabitName && x.CheckinDate == normalizedCheckinDate,
            cancellationToken);
    }

    public async Task<bool> DeleteByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredText(checkinDate, nameof(checkinDate));

        return await DeleteByPredicateAsync(
            x => x.HabitName == normalizedHabitName && x.CheckinDate == normalizedCheckinDate,
            cancellationToken);
    }

    public async Task<bool> DeleteByHabitNameAsync(string habitName, CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));

        return await DeleteByPredicateAsync(x => x.HabitName == normalizedHabitName, cancellationToken);
    }

    private static IQueryable<Checkin> ApplyDateRange(IQueryable<Checkin> query, string? fromDate, string? toDate)
    {
        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            var normalizedFromDate = fromDate.Trim();
            query = query.Where(x => string.CompareOrdinal(x.CheckinDate, normalizedFromDate) >= 0);
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            var normalizedToDate = toDate.Trim();
            query = query.Where(x => string.CompareOrdinal(x.CheckinDate, normalizedToDate) <= 0);
        }

        return query;
    }

    protected override Expression<Func<Checkin, bool>> BuildKeyPredicate(CheckinKey key)
    {
        var normalizedHabitName = NormalizeRequiredText(key.HabitName, nameof(key.HabitName));
        var normalizedCheckinDate = NormalizeRequiredText(key.CheckinDate, nameof(key.CheckinDate));
        return x => x.HabitName == normalizedHabitName && x.CheckinDate == normalizedCheckinDate;
    }
}
