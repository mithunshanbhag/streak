using Streak.Ui.Models.Storage;

namespace Streak.Ui.Repositories.Interfaces;

public interface ICoreRepository
{
    Task<IReadOnlyList<Habit>> GetHabitsAsync(CancellationToken cancellationToken = default);

    Task<Habit?> GetHabitByIdAsync(string habitId, CancellationToken cancellationToken = default);

    Task<Habit?> GetHabitByNameAsync(string habitName, CancellationToken cancellationToken = default);

    Task<int> GetHabitCountAsync(CancellationToken cancellationToken = default);

    Task AddHabitAsync(Habit habit, CancellationToken cancellationToken = default);

    Task UpdateHabitAsync(Habit habit, CancellationToken cancellationToken = default);

    Task DeleteHabitAsync(Habit habit, CancellationToken cancellationToken = default);

    Task UpdateHabitOrderAsync(IReadOnlyCollection<Habit> habits, CancellationToken cancellationToken = default);

    Task<Checkin?> GetCheckinAsync(string habitId, string checkinDate, CancellationToken cancellationToken = default);

    Task UpsertCheckinAsync(Checkin checkin, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Checkin>> GetCheckinsForHabitAsync(
        string habitId,
        string? fromDateInclusive = null,
        string? toDateInclusive = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Checkin>> GetCheckinsForHabitsAsync(
        IReadOnlyCollection<string> habitIds,
        string? fromDateInclusive = null,
        string? toDateInclusive = null,
        CancellationToken cancellationToken = default);

    Task<AppSetting> GetReminderSettingsAsync(CancellationToken cancellationToken = default);

    Task UpdateReminderSettingsAsync(AppSetting appSetting, CancellationToken cancellationToken = default);
}