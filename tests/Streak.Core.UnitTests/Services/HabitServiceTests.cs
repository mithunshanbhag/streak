namespace Streak.Core.UnitTests.Services;

public class HabitServiceTests
{
    #region Boundary tests

    [Fact]
    public async Task GetByIdAsync_ShouldThrowArgumentOutOfRangeException_WhenIdIsNotPositive()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.GetByIdAsync(0);

        var exception = await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        exception.Which.ParamName.Should().Be("id");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateHabit_WhenExistingCountIsOneBelowConfiguredMaximum()
    {
        var existingHabits = Enumerable.Range(1, CoreConstants.MaxHabitCount - 1)
            .Select(index => new Habit { Id = index, Name = $"Habit {index}" })
            .ToList();

        Habit? persistedHabit = null;
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);
        habitRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()))
            .Callback<Habit, CancellationToken>((habit, _) => persistedHabit = habit)
            .ReturnsAsync(true);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var result = await sut.CreateAsync(new Habit { Id = 0, Name = "Stretch", Emoji = "🧘", Description = "Move for a few minutes." });

        result.Id.Should().Be(CoreConstants.MaxHabitCount);
        result.Name.Should().Be("Stretch");
        result.Emoji.Should().Be("🧘");
        result.Description.Should().Be("Move for a few minutes.");
        persistedHabit.Should().BeEquivalentTo(result);
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Positive tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnHabitsOrderedByNameIgnoringCase()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Zulu" },
            new() { Id = 2, Name = "beta" },
            new() { Id = 3, Name = "Alpha" }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var result = await sut.GetAllAsync();

        result.Select(x => x.Name).Should().Equal("Alpha", "beta", "Zulu");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnHabit_WhenIdExists()
    {
        var expectedHabit = new Habit { Id = 3, Name = "Read", Emoji = "📖" };
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedHabit);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var result = await sut.GetByIdAsync(3);

        result.Should().BeEquivalentTo(expectedHabit);
        habitRepositoryMock.Verify(x => x.GetAsync(3, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnHabit_WhenNameMatchesIgnoringCase()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run" },
            new() { Id = 2, Name = "Read" }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var result = await sut.GetByNameAsync("  rEaD ");

        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.Name.Should().Be("Read");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetCountAsync_ShouldReturnRepositoryCount()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var result = await sut.GetCountAsync();

        result.Should().Be(4);
        habitRepositoryMock.Verify(x => x.GetCountAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateHabitWithNextId_WhenInputIsValid()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", Emoji = "🏃" },
            new() { Id = 4, Name = "Read", Emoji = "📖" }
        };

        Habit? persistedHabit = null;
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);
        habitRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()))
            .Callback<Habit, CancellationToken>((habit, _) => persistedHabit = habit)
            .ReturnsAsync(true);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToCreate = new Habit
        {
            Id = 99,
            Name = "  Meditate  ",
            Emoji = "  🧘  ",
            Description = "  Focus on breathing.\nRelax shoulders.  "
        };

        var result = await sut.CreateAsync(habitToCreate);

        result.Should().BeEquivalentTo(
            new Habit
            {
                Id = 5,
                Name = "Meditate",
                Emoji = "🧘",
                Description = "Focus on breathing.\nRelax shoulders."
            });

        persistedHabit.Should().NotBeNull();
        persistedHabit.Should().BeEquivalentTo(result);
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateAndPersistHabit_WhenInputIsValid()
    {
        var existingHabit = new Habit { Id = 2, Name = "Read", Emoji = "📖" };
        var existingHabits = new List<Habit>
        {
            existingHabit,
            new() { Id = 1, Name = "Run", Emoji = "🏃" }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabit);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);
        habitRepositoryMock
            .Setup(x => x.UpdateAsync(existingHabit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var updatedInput = new Habit
        {
            Id = 2,
            Name = "  Read Daily  ",
            Emoji = "  📚  ",
            Description = "  Read before bed.\nTrack pages.  "
        };

        var result = await sut.UpdateAsync(updatedInput);

        result.Should().BeSameAs(existingHabit);
        result.Name.Should().Be("Read Daily");
        result.Emoji.Should().Be("📚");
        result.Description.Should().Be("Read before bed.\nTrack pages.");
        habitRepositoryMock.Verify(x => x.GetAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.UpdateAsync(existingHabit, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldAllowSameHabitName_WhenOnlyCurrentHabitMatchesIgnoringCase()
    {
        var existingHabit = new Habit { Id = 2, Name = "Read", Emoji = "📖" };
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", Emoji = "🏃" },
            existingHabit
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabit);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);
        habitRepositoryMock
            .Setup(x => x.UpdateAsync(existingHabit, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var result = await sut.UpdateAsync(new Habit { Id = 2, Name = "  rEaD  ", Emoji = "📚" });

        result.Name.Should().Be("rEaD");
        result.Emoji.Should().Be("📚");
        habitRepositoryMock.Verify(x => x.GetAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.UpdateAsync(existingHabit, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteHabit_WhenHabitExists()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.ExistsAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        habitRepositoryMock
            .Setup(x => x.DeleteAsync(4, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        await sut.DeleteAsync(4);

        habitRepositoryMock.Verify(x => x.ExistsAsync(4, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.DeleteAsync(4, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task GetByNameAsync_ShouldThrowArgumentException_WhenNameIsWhitespace()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.GetByNameAsync("  ");

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("name");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentNullException_WhenHabitIsNull()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.CreateAsync(null!);

        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("habit");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenHabitNameIsInvalid()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit { Id = 1, Name = "   ", Emoji = "📖" };

        var act = () => sut.CreateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Name");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenDuplicateNameExistsIgnoringCase()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Read" }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToCreate = new Habit { Id = 2, Name = "  rEaD  ", Emoji = "📚" };

        var act = () => sut.CreateAsync(habitToCreate);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("already exists");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()), Times.Never);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowInvalidOperationException_WhenHabitLimitIsReached()
    {
        var existingHabits = Enumerable.Range(1, CoreConstants.MaxHabitCount)
            .Select(index => new Habit { Id = index, Name = $"Habit {index}" })
            .ToList();

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToCreate = new Habit { Id = 7, Name = "New Habit", Emoji = "✨" };

        var act = () => sut.CreateAsync(habitToCreate);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain($"Cannot create more than {CoreConstants.MaxHabitCount} habits.");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()), Times.Never);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenHabitNameExceedsConfiguredMaximum()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit
        {
            Id = 1,
            Name = new string('R', CoreConstants.HabitNameMaxLength + 1),
            Emoji = "📖"
        };

        var act = () => sut.CreateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Name");
        exception.Which.Message.Should().Contain(
            $"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("📚📖")]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenEmojiIsNotASingleEmoji(string emoji)
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit
        {
            Id = 1,
            Name = "Read",
            Emoji = emoji
        };

        var act = () => sut.CreateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Emoji");
        exception.Which.Message.Should().Contain("single emoji");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task CreateAsync_ShouldThrowArgumentException_WhenDescriptionExceedsConfiguredMaximum()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit
        {
            Id = 1,
            Name = "Read",
            Description = new string('D', CoreConstants.HabitDescriptionMaxLength + 1)
        };

        var act = () => sut.CreateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Description");
        exception.Which.Message.Should().Contain($"{CoreConstants.HabitDescriptionMaxLength}");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentNullException_WhenHabitIsNull()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.UpdateAsync(null!);

        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("habit");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenHabitNameIsInvalid()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit { Id = 1, Name = "   ", Emoji = "📖" };

        var act = () => sut.UpdateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Name");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("📚📖")]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenEmojiIsNotASingleEmoji(string emoji)
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit
        {
            Id = 1,
            Name = "Read",
            Emoji = emoji
        };

        var act = () => sut.UpdateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Emoji");
        exception.Which.Message.Should().Contain("single emoji");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowArgumentException_WhenDescriptionExceedsConfiguredMaximum()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var invalidHabit = new Habit
        {
            Id = 1,
            Name = "Read",
            Description = new string('D', CoreConstants.HabitDescriptionMaxLength + 1)
        };

        var act = () => sut.UpdateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Description");
        exception.Which.Message.Should().Contain($"{CoreConstants.HabitDescriptionMaxLength}");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowKeyNotFoundException_WhenHabitDoesNotExist()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAsync(42, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Habit?)null);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToUpdate = new Habit { Id = 42, Name = "Read", Emoji = "📚" };

        var act = () => sut.UpdateAsync(habitToUpdate);

        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Contain("42");
        habitRepositoryMock.Verify(x => x.GetAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenNameConflictsWithAnotherHabit()
    {
        var existingHabit = new Habit { Id = 2, Name = "Read", Emoji = "📖" };
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", Emoji = "🏃" },
            existingHabit
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabit);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToUpdate = new Habit { Id = 2, Name = " RUN ", Emoji = "🏃‍♂️" };

        var act = () => sut.UpdateAsync(habitToUpdate);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("already exists");
        habitRepositoryMock.Verify(x => x.GetAsync(2, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()), Times.Never);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task DeleteAsync_ShouldThrowKeyNotFoundException_WhenHabitDoesNotExist()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.ExistsAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.DeleteAsync(8);

        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Contain("8");
        habitRepositoryMock.Verify(x => x.ExistsAsync(8, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    #endregion
}