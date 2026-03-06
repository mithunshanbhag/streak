using Streak.Core.Services.Implementations;
using Streak.Core.Services.Interfaces;

namespace Streak.Core.UnitTests.Services;

public class HabitServiceTests
{
    #region Positive tests

    [Fact]
    public async Task GetAllAsync_ShouldReturnHabitsOrderedByDisplayOrderThenName()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Zulu", DisplayOrder = 2 },
            new() { Id = 2, Name = "beta", DisplayOrder = 1 },
            new() { Id = 3, Name = "Alpha", DisplayOrder = 1 }
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
        var expectedHabit = new Habit { Id = 3, Name = "Read", Emoji = "📖", DisplayOrder = 2 };
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
            new() { Id = 1, Name = "Run", DisplayOrder = 1 },
            new() { Id = 2, Name = "Read", DisplayOrder = 2 }
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
    public async Task CreateAsync_ShouldCreateHabitWithNextIdAndDisplayOrder_WhenInputIsValid()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", Emoji = "🏃", DisplayOrder = 2 },
            new() { Id = 4, Name = "Read", Emoji = "📖", DisplayOrder = 5 }
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
        var habitToCreate = new Habit { Id = 99, Name = "  Meditate  ", Emoji = "  🧘  ", DisplayOrder = 0 };

        var result = await sut.CreateAsync(habitToCreate);

        result.Should().BeEquivalentTo(
            new Habit
            {
                Id = 5,
                Name = "Meditate",
                Emoji = "🧘",
                DisplayOrder = 6
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
        var existingHabit = new Habit { Id = 2, Name = "Read", Emoji = "📖", DisplayOrder = 3 };
        var existingHabits = new List<Habit>
        {
            existingHabit,
            new() { Id = 1, Name = "Run", Emoji = "🏃", DisplayOrder = 1 }
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
        var updatedInput = new Habit { Id = 2, Name = "  Read Daily  ", Emoji = "  📚  ", DisplayOrder = 999 };

        var result = await sut.UpdateAsync(updatedInput);

        result.Should().BeSameAs(existingHabit);
        result.Name.Should().Be("Read Daily");
        result.Emoji.Should().Be("📚");
        result.DisplayOrder.Should().Be(3);
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

    [Fact]
    public async Task ReorderAsync_ShouldApplySequentialDisplayOrder_WhenInputMatchesExistingHabits()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", DisplayOrder = 20 },
            new() { Id = 2, Name = "Read", DisplayOrder = 30 },
            new() { Id = 3, Name = "Code", DisplayOrder = 10 }
        };

        IReadOnlyList<Habit>? reorderedHabits = null;
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);
        habitRepositoryMock
            .Setup(x => x.ReorderAsync(It.IsAny<IReadOnlyList<Habit>>(), It.IsAny<CancellationToken>()))
            .Callback<IReadOnlyList<Habit>, CancellationToken>((habits, _) => reorderedHabits = habits)
            .ReturnsAsync(true);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        await sut.ReorderAsync([3, 1, 2]);

        reorderedHabits.Should().NotBeNull();
        reorderedHabits!.Select(x => x.Id).Should().Equal(3, 1, 2);
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.ReorderAsync(It.IsAny<IReadOnlyList<Habit>>(), It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_ShouldReturnWithoutUpdates_WhenNoHabitsExistAndInputIsEmpty()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        await sut.ReorderAsync([]);

        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        var invalidHabit = new Habit { Id = 1, Name = "   ", Emoji = "📖", DisplayOrder = 1 };

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
            new() { Id = 1, Name = "Read", DisplayOrder = 1 }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToCreate = new Habit { Id = 2, Name = "  rEaD  ", Emoji = "📚", DisplayOrder = 2 };

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
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Habit 1", DisplayOrder = 1 },
            new() { Id = 2, Name = "Habit 2", DisplayOrder = 2 },
            new() { Id = 3, Name = "Habit 3", DisplayOrder = 3 },
            new() { Id = 4, Name = "Habit 4", DisplayOrder = 4 },
            new() { Id = 5, Name = "Habit 5", DisplayOrder = 5 },
            new() { Id = 6, Name = "Habit 6", DisplayOrder = 6 }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);
        var habitToCreate = new Habit { Id = 7, Name = "New Habit", Emoji = "✨", DisplayOrder = 7 };

        var act = () => sut.CreateAsync(habitToCreate);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("Cannot create more than 6 habits.");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Habit>(), It.IsAny<CancellationToken>()), Times.Never);
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
        var invalidHabit = new Habit { Id = 1, Name = "   ", Emoji = "📖", DisplayOrder = 1 };

        var act = () => sut.UpdateAsync(invalidHabit);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("Name");
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
        var habitToUpdate = new Habit { Id = 42, Name = "Read", Emoji = "📚", DisplayOrder = 1 };

        var act = () => sut.UpdateAsync(habitToUpdate);

        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Contain("42");
        habitRepositoryMock.Verify(x => x.GetAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowInvalidOperationException_WhenNameConflictsWithAnotherHabit()
    {
        var existingHabit = new Habit { Id = 2, Name = "Read", Emoji = "📖", DisplayOrder = 2 };
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", Emoji = "🏃", DisplayOrder = 1 },
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
        var habitToUpdate = new Habit { Id = 2, Name = " RUN ", Emoji = "🏃‍♂️", DisplayOrder = 2 };

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

    [Fact]
    public async Task ReorderAsync_ShouldThrowArgumentNullException_WhenHabitIdsAreNull()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.ReorderAsync(null!);

        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.Which.ParamName.Should().Be("habitIdsInDisplayOrder");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_ShouldThrowArgumentException_WhenHabitIdsContainDuplicates()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.ReorderAsync([1, 1]);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("habitIdsInDisplayOrder");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_ShouldThrowArgumentException_WhenInputIsEmptyButHabitsExist()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Read", DisplayOrder = 1 }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.ReorderAsync([]);

        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.Which.ParamName.Should().Be("habitIdsInDisplayOrder");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.ReorderAsync(It.IsAny<IReadOnlyList<Habit>>(), It.IsAny<CancellationToken>()), Times.Never);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_ShouldThrowKeyNotFoundException_WhenInputContainsUnknownHabitId()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", DisplayOrder = 1 },
            new() { Id = 2, Name = "Read", DisplayOrder = 2 },
            new() { Id = 3, Name = "Code", DisplayOrder = 3 }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.ReorderAsync([1, 2, 99]);

        var exception = await act.Should().ThrowAsync<KeyNotFoundException>();
        exception.Which.Message.Should().Contain("99");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.ReorderAsync(It.IsAny<IReadOnlyList<Habit>>(), It.IsAny<CancellationToken>()), Times.Never);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ReorderAsync_ShouldThrowInvalidOperationException_WhenAtomicReorderFails()
    {
        var existingHabits = new List<Habit>
        {
            new() { Id = 1, Name = "Run", DisplayOrder = 1 },
            new() { Id = 2, Name = "Read", DisplayOrder = 2 },
            new() { Id = 3, Name = "Code", DisplayOrder = 3 }
        };

        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        habitRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingHabits);
        habitRepositoryMock
            .Setup(x => x.ReorderAsync(It.IsAny<IReadOnlyList<Habit>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.ReorderAsync([3, 1, 2]);

        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.Which.Message.Should().Contain("Failed to reorder habits.");
        habitRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.Verify(x => x.ReorderAsync(It.IsAny<IReadOnlyList<Habit>>(), It.IsAny<CancellationToken>()), Times.Once);
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    #endregion

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
    public async Task ReorderAsync_ShouldThrowArgumentOutOfRangeException_WhenHabitIdsContainNonPositiveValue()
    {
        var habitRepositoryMock = new Mock<IHabitRepository>(MockBehavior.Strict);
        IHabitService sut = new HabitService(habitRepositoryMock.Object);

        var act = () => sut.ReorderAsync([2, 0, 1]);

        var exception = await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        exception.Which.ParamName.Should().Be("habitIdsInDisplayOrder");
        habitRepositoryMock.VerifyNoOtherCalls();
    }

    #endregion
}