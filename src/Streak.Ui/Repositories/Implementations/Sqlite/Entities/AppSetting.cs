using System;
using System.Collections.Generic;

namespace Streak.Ui.Repositories.Implementations.Sqlite.Entities;

public partial class AppSetting
{
    public int Id { get; set; }

    public int IsReminderEnabled { get; set; }

    public TimeSpan ReminderTimeLocal { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
