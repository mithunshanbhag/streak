namespace Streak.Core.UnitTests.Repositories;

public class HabitRepositoryTests
{
    #region Positive tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnHabitsOrderedByDisplayOrderThenName()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.AddRange(
                new Habit { Id = 2, Name = "Read", DisplayOrder = 3 },
                new Habit { Id = 1, Name = "Meditate", DisplayOrder = 1 },
                new Habit { Id = 3, Name = "Code", DisplayOrder = 2 });
            await context.SaveChangesAsync();

            ISqlGenericRepository<Habit, int> sut = new HabitRepository(context);

            var result = await sut.GetAllAsync();

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
            var habit = new Habit { Id = 1, Name = "Read", Emoji = "📖", DisplayOrder = 1 };

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
            context.Habits.Add(new Habit { Id = 1, Name = "Read", Emoji = "📖", DisplayOrder = 1 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            ISqlGenericRepository<Habit, int> sut = new HabitRepository(context);
            var updatedHabit = new Habit { Id = 1, Name = "Read Daily", Emoji = "📚", DisplayOrder = 3 };

            var result = await sut.UpdateAsync(updatedHabit);

            result.Should().BeTrue();
            (await context.Habits.CountAsync()).Should().Be(1);
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
