using System;
using System.Collections.Generic;

namespace Streak.Ui.Repositories.Implementations.Sqlite.Entities;

public partial class Checkin
{
    public string Id { get; set; } = null!;

    public string HabitId { get; set; } = null!;

    public string CheckinDate { get; set; } = null!;

    public int IsDone { get; set; }

    public string CreatedAtUtc { get; set; } = null!;

    public string? UpdatedAtUtc { get; set; }

    public virtual Habit Habit { get; set; } = null!;
}
