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

        return await ApplyDateRange(
                Query().Where(x => normalizedHabitNames.Contains(x.HabitNavigation.Name)),
                fromDate,
                toDate)
            .OrderBy(x => x.HabitNavigation.Name)
            .ThenByDescending(x => x.CheckinDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Checkin>> GetByHabitIdsAsync(
        IReadOnlyCollection<int> habitIds,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(habitIds);

        if (habitIds.Count == 0) return [];

        var normalizedHabitIds = habitIds
            .Distinct()
            .ToArray();

        if (normalizedHabitIds.Any(x => x <= 0))
            throw new ArgumentOutOfRangeException(nameof(habitIds), "Habit IDs must be greater than zero.");

        return await ApplyDateRange(
                Query().Where(x => normalizedHabitIds.Contains(x.HabitId)),
                fromDate,
                toDate)
            .OrderBy(x => x.HabitId)
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

    private static IQueryable<Checkin> ApplyDateRange(
        IQueryable<Checkin> checkins,
        string? fromDate,
        string? toDate)
    {
        if (!string.IsNullOrWhiteSpace(fromDate))
        {
            var normalizedFromDate = fromDate.Trim();
            checkins = checkins.Where(x => string.Compare(x.CheckinDate, normalizedFromDate) >= 0);
        }

        if (!string.IsNullOrWhiteSpace(toDate))
        {
            var normalizedToDate = toDate.Trim();
            checkins = checkins.Where(x => string.Compare(x.CheckinDate, normalizedToDate) <= 0);
        }

        return checkins;
    }

    protected override Expression<Func<Checkin, bool>> BuildKeyPredicate(CheckinKey key)
    {
        var normalizedCheckinDate = NormalizeRequiredText(key.CheckinDate, nameof(key.CheckinDate));
        return x => x.HabitId == key.HabitId && x.CheckinDate == normalizedCheckinDate;
    }
}
