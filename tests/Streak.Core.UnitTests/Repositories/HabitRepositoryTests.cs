namespace Streak.Core.UnitTests.Repositories;

public class HabitRepositoryTests
{
    #region Positive tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPersistedHabits()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.AddRange(
                new Habit { Id = 2, Name = "Read" },
                new Habit { Id = 1, Name = "Meditate" },
                new Habit { Id = 3, Name = "Code" });
            await context.SaveChangesAsync();

            ISqlGenericRepository<Habit, int> sut = new HabitRepository(context);

            var result = await sut.GetAllAsync();

            result.Should().HaveCount(3);
            result.Select(x => x.Name).Should().BeEquivalentTo("Meditate", "Code", "Read");
        }
    }

    [Fact]
    public async Task AddAsync_ShouldPersistHabitAndReturnTrue()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.ChangeTracker.Clear();
            ISqlGenericRepository<Habit, int> sut = new HabitRepository(context);
            var habit = new Habit { Id = 1, Name = "Read", Emoji = "📖" };

            var result = await sut.AddAsync(habit);

            result.Should().BeTrue();
            var savedHabit = await context.Habits.SingleAsync();
            savedHabit.Name.Should().Be("Read");
            savedHabit.Emoji.Should().Be("📖");
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingHabitAndReturnTrue()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Read", Emoji = "📖" });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            ISqlGenericRepository<Habit, int> sut = new HabitRepository(context);
            var updatedHabit = new Habit { Id = 1, Name = "Read Daily", Emoji = "📚" };

            var result = await sut.UpdateAsync(updatedHabit);

            result.Should().BeTrue();
            (await context.Habits.CountAsync()).Should().Be(1);
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldCascadeDeleteRelatedCheckins_WhenHabitExists()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Read", Emoji = "📖" });
            context.Checkins.AddRange(
                new Checkin { HabitId = 1, CheckinDate = "2026-03-20" },
                new Checkin { HabitId = 1, CheckinDate = "2026-03-21" });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            IHabitRepository sut = new HabitRepository(context);

            var result = await sut.DeleteAsync(1);

            result.Should().BeTrue();
            (await context.Habits.CountAsync()).Should().Be(0);
            (await context.Checkins.CountAsync()).Should().Be(0);
        }
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenHabitDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            IHabitRepository sut = new HabitRepository(context);

            var result = await sut.GetAsync(999);

            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnFalse_WhenHabitDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            IHabitRepository sut = new HabitRepository(context);

            var result = await sut.DeleteAsync(999);

            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetByNameAsync_ShouldThrowArgumentException_WhenNameIsWhitespace()
    {
        var dbContextMock = new Mock<StreakDbContext>(new DbContextOptionsBuilder<StreakDbContext>().Options);
        IHabitRepository sut = new HabitRepository(dbContextMock.Object);

        var act = () => sut.GetByNameAsync("   ");

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("name");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowArgumentNullException_WhenHabitIsNull()
    {
        var dbContextMock = new Mock<StreakDbContext>(new DbContextOptionsBuilder<StreakDbContext>().Options);
        IHabitRepository sut = new HabitRepository(dbContextMock.Object);

        var act = () => sut.AddAsync(null!);

        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("entity");
    }

    #endregion

    #region Boundary tests

    [Fact]
    public async Task GetByIdsAsync_ShouldReturnEmpty_WhenIdsCollectionIsEmpty()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            IHabitRepository sut = new HabitRepository(context);

            var result = await sut.GetByKeysAsync([]);

            result.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task AddRangeAsync_ShouldReturnFalse_WhenHabitsCollectionIsEmpty()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            var sut = new HabitRepository(context);

            var result = await sut.AddRangeAsync([]);

            result.Should().BeFalse();
            (await context.Habits.CountAsync()).Should().Be(0);
        }
    }

    #endregion
}
