namespace Streak.Core.UnitTests.Repositories;

public class CheckinRepositoryTests
{
    #region Boundary tests

    [Fact]
    public async Task GetByHabitNamesAsync_ShouldReturnEmpty_WhenHabitNamesCollectionIsEmpty()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            var sut = new CheckinRepository(context);

            var result = await sut.GetByHabitNamesAsync([]);

            result.Should().BeEmpty();
        }
    }

    #endregion

    #region Positive tests

    [Fact]
    public async Task GetByHabitNamesAsync_ShouldFilterByNames_AndReturnExpectedOrder()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.AddRange(
                new Habit { Id = 1, Name = "Run" },
                new Habit { Id = 2, Name = "Read" });

            context.Checkins.AddRange(
                new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 1 },
                new Checkin { HabitId = 1, CheckinDate = "2025-01-03", IsDone = 1 },
                new Checkin { HabitId = 2, CheckinDate = "2025-01-02", IsDone = 1 },
                new Checkin { HabitId = 2, CheckinDate = "2025-01-03", IsDone = 0 });
            await context.SaveChangesAsync();

            ICheckinRepository sut = new CheckinRepository(context);

            var result = await sut.GetByHabitNamesAsync([" Run ", "Read", "Run"]);

            result.Select(x => $"{x.HabitId}:{x.CheckinDate}")
                .Should()
                .Equal("2:2025-01-03", "2:2025-01-02", "1:2025-01-03", "1:2025-01-01");
        }
    }

    [Fact]
    public async Task AddAsync_ShouldPersistCheckinAndReturnTrue()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            ISqlGenericRepository<Checkin, CheckinKey> sut = new CheckinRepository(context);
            var checkin = new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 1 };

            var result = await sut.AddAsync(checkin);

            result.Should().BeTrue();
            (await context.Checkins.CountAsync()).Should().Be(1);
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingCheckinAndReturnTrue()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            context.Checkins.Add(new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 0 });
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            ISqlGenericRepository<Checkin, CheckinKey> sut = new CheckinRepository(context);
            var updatedCheckin = new Checkin
            {
                HabitId = 1,
                CheckinDate = "2025-01-01",
                IsDone = 1
            };

            var result = await sut.UpdateAsync(updatedCheckin);

            result.Should().BeTrue();
            context.ChangeTracker.Clear();
            var savedCheckin = await context.Checkins.SingleAsync();
            savedCheckin.IsDone.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCheckin_WhenKeyMatchesExistingCheckin()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            context.Checkins.Add(new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 1 });
            await context.SaveChangesAsync();

            ISqlGenericRepository<Checkin, CheckinKey> sut = new CheckinRepository(context);

            var result = await sut.GetAsync(new CheckinKey(1, " 2025-01-01 "));

            result.Should().NotBeNull();
            result.HabitId.Should().Be(1);
            result.CheckinDate.Should().Be("2025-01-01");
        }
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldDeleteExistingCheckinAndReturnTrue()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            context.Checkins.Add(new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 1 });
            await context.SaveChangesAsync();

            ISqlGenericRepository<Checkin, CheckinKey> sut = new CheckinRepository(context);

            var result = await sut.DeleteAsync(new CheckinKey(1, "2025-01-01"));

            result.Should().BeTrue();
            (await context.Checkins.CountAsync()).Should().Be(0);
        }
    }

    [Fact]
    public async Task DeleteByHabitNameAndDateAsync_ShouldDeleteExistingCheckinAndReturnTrue()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            context.Checkins.Add(new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 1 });
            await context.SaveChangesAsync();

            var sut = new CheckinRepository(context);

            var result = await sut.DeleteByHabitNameAndDateAsync(" Run ", "2025-01-01");

            result.Should().BeTrue();
            (await context.Checkins.CountAsync()).Should().Be(0);
        }
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task GetByHabitNameAndDateAsync_ShouldReturnNull_WhenCheckinDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            await context.SaveChangesAsync();

            var sut = new CheckinRepository(context);

            var result = await sut.GetByHabitNameAndDateAsync("Run", "2025-01-01");

            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteByIdAsync_ShouldReturnFalse_WhenCheckinDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            context.Checkins.Add(new Checkin { HabitId = 1, CheckinDate = "2025-01-01", IsDone = 1 });
            await context.SaveChangesAsync();

            var sut = new CheckinRepository(context);

            var result = await sut.DeleteAsync(new CheckinKey(999, "2025-01-01"));

            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ExistsByIdAsync_ShouldReturnFalse_WhenCheckinDoesNotExist()
    {
        await using var context = TestDbContextFactory.CreateContext(out var connection);
        await using (connection)
        {
            context.Habits.Add(new Habit { Id = 1, Name = "Run" });
            await context.SaveChangesAsync();

            ISqlGenericRepository<Checkin, CheckinKey> sut = new CheckinRepository(context);

            var result = await sut.ExistsAsync(new CheckinKey(1, "2025-01-01"));

            result.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetByHabitNameAsync_ShouldThrowArgumentException_WhenHabitNameIsWhitespace()
    {
        var dbContextMock = new Mock<StreakDbContext>(new DbContextOptionsBuilder<StreakDbContext>().Options);
        var sut = new CheckinRepository(dbContextMock.Object);

        var act = () => sut.GetByHabitNameAsync(" ");

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("habitName");
    }

    [Fact]
    public async Task AddAsync_ShouldThrowArgumentNullException_WhenCheckinIsNull()
    {
        var dbContextMock = new Mock<StreakDbContext>(new DbContextOptionsBuilder<StreakDbContext>().Options);
        var sut = new CheckinRepository(dbContextMock.Object);

        var act = () => sut.AddAsync(null!);

        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("entity");
    }

    #endregion
}
