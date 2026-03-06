namespace Streak.Core.Repositories.DbContexts;

public partial class StreakDbContext(DbContextOptions<StreakDbContext> options) : DbContext(options)
{
    public virtual DbSet<AppSetting> AppSettings { get; set; }

    public virtual DbSet<Checkin> Checkins { get; set; }

    public virtual DbSet<Habit> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppSetting>(entity => { entity.HasKey(e => e.Key); });

        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => new { e.HabitName, e.CheckinDate });

            entity.Property(e => e.LastUpdatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.HabitNameNavigation).WithMany(p => p.Checkins)
                .HasPrincipalKey(p => p.Name)
                .HasForeignKey(d => d.HabitName)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.HasIndex(e => e.DisplayOrder, "IX_Habits_DisplayOrder").IsUnique();

            entity.HasIndex(e => e.Name, "IX_Habits_Name").IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}