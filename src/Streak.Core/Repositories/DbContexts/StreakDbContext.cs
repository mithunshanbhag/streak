namespace Streak.Core.Repositories.DbContexts;

public partial class StreakDbContext(DbContextOptions<StreakDbContext> options) : DbContext(options)
{
    public virtual DbSet<Checkin> Checkins { get; set; }

    public virtual DbSet<Habit> Habits { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Checkin>(entity =>
        {
            entity.HasKey(e => new { e.HabitId, e.CheckinDate });

            entity.ToTable(tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Checkins_CheckinDate",
                    "length (CheckinDate) = 10 AND CheckinDate GLOB '[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]' AND strftime ('%Y-%m-%d', CheckinDate) IS NOT NULL AND strftime ('%Y-%m-%d', CheckinDate) = CheckinDate");
            });

            entity.HasOne(d => d.HabitNavigation).WithMany(p => p.Checkins)
                .HasForeignKey(d => d.HabitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.ToTable(tableBuilder =>
            {
                tableBuilder.HasCheckConstraint("CK_Habits_Name_Length", "length (trim(Name)) BETWEEN 1 AND 30");
            });

            entity.Property(e => e.Name).UseCollation("NOCASE");
            entity.HasIndex(e => e.Name, "IX_Habits_Name").IsUnique();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
