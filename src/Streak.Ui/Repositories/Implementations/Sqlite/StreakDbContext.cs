using Microsoft.EntityFrameworkCore;
using Streak.Ui.Models.Storage;

namespace Streak.Ui.Repositories.Implementations.Sqlite;

public partial class StreakDbContext : DbContext
{
    public StreakDbContext(DbContextOptions<StreakDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<Checkin> Checkins { get; set; }

    public virtual DbSet<Habit> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.IsReminderEnabled).HasDefaultValue(1);
            entity.Property(e => e.ReminderTimeLocal).HasDefaultValueSql("'21:00:00'");
            entity.Property(e => e.UpdatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasIndex(e => new { e.HabitId, e.CheckinDate }, "IX_Checkins_HabitId_CheckinDate").IsUnique();

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsDone).HasDefaultValue(1);

            entity.HasOne(d => d.Habit).WithMany(p => p.Checkins).HasForeignKey(d => d.HabitId);
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasIndex(e => e.Name, "IX_Habits_Name").IsUnique();

            entity.Property(e => e.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Name).UseCollation("NOCASE");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}