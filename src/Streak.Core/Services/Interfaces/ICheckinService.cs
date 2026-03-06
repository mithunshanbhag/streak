namespace Streak.Core.Services.Interfaces;

public interface ICheckinService
{
    Task<Checkin?> GetByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Checkin>> GetHistoryAsync(
        string habitName,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default);

    Task<Checkin> UpsertAsync(Checkin checkin, CancellationToken cancellationToken = default);

    Task<Checkin> ToggleForTodayAsync(
        string habitName,
        bool isDone,
        CancellationToken cancellationToken = default);

    Task DeleteForHabitAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default);

    Task<int> GetCurrentStreakAsync(string habitName, CancellationToken cancellationToken = default);
}