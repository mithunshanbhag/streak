namespace Streak.Ui.Services.Interfaces;

public interface IReminderNotifier
{
    /// <summary>
    ///     Posts a reminder notification for the supplied count of pending habits.
    /// </summary>
    /// <param name="pendingHabitCount">The number of habits that are still unchecked for today.</param>
    void NotifyPendingHabits(int pendingHabitCount);
}
