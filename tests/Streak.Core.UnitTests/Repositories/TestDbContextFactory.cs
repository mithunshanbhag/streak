namespace Streak.Core.UnitTests.Repositories;

internal static class TestDbContextFactory
{
    public static StreakDbContext CreateContext(out SqliteConnection connection)
    {
        connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<StreakDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new StreakDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}