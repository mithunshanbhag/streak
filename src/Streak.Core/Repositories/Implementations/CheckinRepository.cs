namespace Streak.Core.Repositories.Implementations;

public class CheckinRepository(StreakDbContext dbContext) : SqlGenericRepositoryBase<Checkin, CheckinKey>(dbContext), ICheckinRepository
{
    public async Task<IReadOnlyList<Checkin>> GetByHabitNameAsync(string habitName, CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));

        return await Query()
            .Where(x => x.HabitNavigation.Name == normalizedHabitName)
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

        if (habitNames.Count == 0) return [];

        var normalizedHabitNames = habitNames
            .Select(x => NormalizeRequiredText(x, nameof(habitNames)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var query = Query().Where(x => normalizedHabitNames.Contains(x.HabitNavigation.Name));
        var checkins = await query
            .OrderBy(x => x.HabitNavigation.Name)
            .ThenByDescending(x => x.CheckinDate)
            .ToListAsync(cancellationToken);

        return ApplyDateRange(checkins, fromDate, toDate);
    }

    public async Task<Checkin?> GetByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredText(checkinDate, nameof(checkinDate));

        return await Query().SingleOrDefaultAsync(
            x => x.HabitNavigation.Name == normalizedHabitName && x.CheckinDate == normalizedCheckinDate,
            cancellationToken);
    }

    public async Task<bool> DeleteByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredText(checkinDate, nameof(checkinDate));
        var habitIds = StreakDbContext.Set<Habit>()
            .Where(x => x.Name == normalizedHabitName)
            .Select(x => x.Id);

        return await DeleteByPredicateAsync(
            x => habitIds.Contains(x.HabitId) && x.CheckinDate == normalizedCheckinDate,
            cancellationToken);
    }

    public async Task<bool> DeleteByHabitNameAsync(string habitName, CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var habitIds = StreakDbContext.Set<Habit>()
            .Where(x => x.Name == normalizedHabitName)
            .Select(x => x.Id);

        return await DeleteByPredicateAsync(x => habitIds.Contains(x.HabitId), cancellationToken);
    }

    private static IReadOnlyList<Checkin> ApplyDateRange(
        IReadOnlyList<Checkin> checkins,
        string? fromDate,
        string? toDate)
    {
        IEnumerable<Checkin> filteredCheckins = checkins;

        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            var normalizedFromDate = fromDate.Trim();
            filteredCheckins = filteredCheckins.Where(x => string.CompareOrdinal(x.CheckinDate, normalizedFromDate) >= 0);
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            var normalizedToDate = toDate.Trim();
            filteredCheckins = filteredCheckins.Where(x => string.CompareOrdinal(x.CheckinDate, normalizedToDate) <= 0);
        }

        return filteredCheckins.ToList();
    }

    protected override Expression<Func<Checkin, bool>> BuildKeyPredicate(CheckinKey key)
    {
        var normalizedCheckinDate = NormalizeRequiredText(key.CheckinDate, nameof(key.CheckinDate));
        return x => x.HabitId == key.HabitId && x.CheckinDate == normalizedCheckinDate;
    }
}
