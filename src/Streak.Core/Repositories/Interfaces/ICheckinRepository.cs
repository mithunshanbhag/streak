namespace Streak.Core.Repositories.Interfaces;

public readonly record struct CheckinKey(string HabitName, string CheckinDate);

public interface ICheckinRepository : ISqlGenericRepository<Checkin, CheckinKey>
{
    Task<IReadOnlyList<Checkin>> GetByHabitNameAsync(string habitName, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Checkin>> GetByHabitNamesAsync(
        IReadOnlyCollection<string> habitNames,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    Task<Checkin?> GetByHabitNameAndDateAsync(string habitName, string checkinDate, CancellationToken cancellationToken = default);

    Task<bool> DeleteByHabitNameAndDateAsync(string habitName, string checkinDate, CancellationToken cancellationToken = default);

    Task<bool> DeleteByHabitNameAsync(string habitName, CancellationToken cancellationToken = default);
}
