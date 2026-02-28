using Microsoft.EntityFrameworkCore;
using Streak.Ui.Constants;
using Streak.Ui.Models.Storage;
using Streak.Ui.Repositories.Implementations.Sqlite;
using Streak.Ui.Repositories.Interfaces;

namespace Streak.Ui.Repositories.Implementations;

public class CoreRepository : ICoreRepository
{
    private readonly StreakDbContext dbContext;

    public CoreRepository(StreakDbContext dbContext)
    {
        this.dbContext = dbContext;
        dbContext.Database.EnsureCreated();
    }

    public async Task<IReadOnlyList<Habit>> GetHabitsAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<Habit?> GetHabitByIdAsync(string habitId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == habitId, cancellationToken);
    }

    public async Task<Habit?> GetHabitByNameAsync(string habitName, CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Name == habitName, cancellationToken);
    }

    public async Task<int> GetHabitCountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Habits.CountAsync(cancellationToken);
    }

    public async Task AddHabitAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateHabitAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        dbContext.Habits.Update(habit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteHabitAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        dbContext.Habits.Remove(habit);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateHabitOrderAsync(IReadOnlyCollection<Habit> habits, CancellationToken cancellationToken = default)
    {
        dbContext.Habits.UpdateRange(habits);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Checkin?> GetCheckinAsync(string habitId, string checkinDate, CancellationToken cancellationToken = default)
    {
        return await dbContext.Checkins
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.HabitId == habitId && x.CheckinDate == checkinDate, cancellationToken);
    }

    public async Task UpsertCheckinAsync(Checkin checkin, CancellationToken cancellationToken = default)
    {
        var existingCheckin = await dbContext.Checkins
            .FirstOrDefaultAsync(x => x.HabitId == checkin.HabitId && x.CheckinDate == checkin.CheckinDate, cancellationToken);

        if (existingCheckin is null)
        {
            dbContext.Checkins.Add(checkin);
        }
        else
        {
            existingCheckin.IsDone = checkin.IsDone;
            existingCheckin.UpdatedAtUtc = checkin.UpdatedAtUtc;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Checkin>> GetCheckinsForHabitAsync(
        string habitId,
        string? fromDateInclusive = null,
        string? toDateInclusive = null,
        CancellationToken cancellationToken = default)
    {
        return (await dbContext.Checkins
                .AsNoTracking()
                .Where(x => x.HabitId == habitId)
                .OrderBy(x => x.CheckinDate)
                .ToListAsync(cancellationToken))
            .Where(x => IsInDateRange(x.CheckinDate, fromDateInclusive, toDateInclusive))
            .ToList();
    }

    public async Task<IReadOnlyList<Checkin>> GetCheckinsForHabitsAsync(
        IReadOnlyCollection<string> habitIds,
        string? fromDateInclusive = null,
        string? toDateInclusive = null,
        CancellationToken cancellationToken = default)
    {
        if (habitIds.Count == 0)
            return [];

        return (await dbContext.Checkins
                .AsNoTracking()
                .Where(x => habitIds.Contains(x.HabitId))
                .OrderBy(x => x.CheckinDate)
                .ToListAsync(cancellationToken))
            .Where(x => IsInDateRange(x.CheckinDate, fromDateInclusive, toDateInclusive))
            .ToList();
    }

    public async Task<AppSetting> GetReminderSettingsAsync(CancellationToken cancellationToken = default)
    {
        var appSetting = await dbContext.AppSettings
            .FirstOrDefaultAsync(x => x.Id == CoreConstants.ReminderSettingsId, cancellationToken);

        if (appSetting is not null)
            return appSetting;

        appSetting = new AppSetting
        {
            Id = CoreConstants.ReminderSettingsId,
            IsReminderEnabled = 1,
            ReminderTimeLocal = TimeSpan.Parse(CoreConstants.DefaultReminderTimeLocal),
            UpdatedAtUtc = DateTime.UtcNow
        };

        dbContext.AppSettings.Add(appSetting);
        await dbContext.SaveChangesAsync(cancellationToken);
        return appSetting;
    }

    public async Task UpdateReminderSettingsAsync(AppSetting appSetting, CancellationToken cancellationToken = default)
    {
        dbContext.AppSettings.Update(appSetting);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsInDateRange(string checkinDate, string? fromDateInclusive, string? toDateInclusive)
    {
        if (!string.IsNullOrWhiteSpace(fromDateInclusive) &&
            string.CompareOrdinal(checkinDate, fromDateInclusive) < 0)
            return false;

        if (!string.IsNullOrWhiteSpace(toDateInclusive) &&
            string.CompareOrdinal(checkinDate, toDateInclusive) > 0)
            return false;

        return true;
    }
}