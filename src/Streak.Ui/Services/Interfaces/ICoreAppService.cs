using Streak.Ui.Models.ViewModels.InputModels;
using Streak.Ui.Models.ViewModels.ResultModels;

namespace Streak.Ui.Services.Interfaces;

public interface ICoreAppService
{
    Task<IReadOnlyList<HabitViewModel>> GetHabitsAsync(CancellationToken cancellationToken = default);

    Task<HabitViewModel> CreateHabitAsync(HabitCreateInputModel inputModel, CancellationToken cancellationToken = default);

    Task<HabitViewModel> UpdateHabitAsync(HabitUpdateInputModel inputModel, CancellationToken cancellationToken = default);

    Task DeleteHabitAsync(string habitId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HabitViewModel>> UpdateHabitOrderAsync(HabitOrderUpdateInputModel inputModel, CancellationToken cancellationToken = default);

    Task<HabitViewModel> ToggleTodayCheckinAsync(HabitToggleCheckinInputModel inputModel, CancellationToken cancellationToken = default);

    Task<HabitTrendViewModel> GetHabitTrendsAsync(HabitTrendQueryInputModel inputModel, CancellationToken cancellationToken = default);

    Task<ReminderSettingsViewModel> GetReminderSettingsAsync(CancellationToken cancellationToken = default);

    Task<ReminderSettingsViewModel> UpdateReminderSettingsAsync(
        ReminderSettingsUpdateInputModel inputModel,
        CancellationToken cancellationToken = default);
}
