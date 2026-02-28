using System;
using System.Collections.Generic;

namespace Streak.Ui.Repositories.Implementations.Sqlite.Entities;

public partial class Habit
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Emoji { get; set; }

    public int SortOrder { get; set; }

    public string CreatedAtUtc { get; set; } = null!;

    public string? UpdatedAtUtc { get; set; }

    public virtual ICollection<Checkin> Checkins { get; set; } = new List<Checkin>();
}
