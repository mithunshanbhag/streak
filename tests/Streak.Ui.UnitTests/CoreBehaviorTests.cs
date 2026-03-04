using System.Globalization;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace Streak.Ui.UnitTests;

public class CoreBehaviorTests
{
    [Fact]
    public async Task HabitNameValidation_ShouldRequireName()
    {
        var validator = new HabitCreateInputModelValidator();

        var result = await validator.ValidateAsync(new HabitCreateInputModel { Name = "   " });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Habit name is required.");
    }

    [Fact]
    public async Task HabitNameValidation_ShouldEnforceMaxLength()
    {
        var validator = new HabitCreateInputModelValidator();

        var result = await validator.ValidateAsync(new HabitCreateInputModel { Name = new string('a', 31) });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.ErrorMessage == $"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.");
    }

    [Fact]
    public async Task CreateHabit_ShouldRejectWhenAtMaxHabitCount()
    {
        var repository = new Mock<ICoreRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetHabitByNameAsync("Workout", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Habit?)null);
        repository.Setup(x => x.GetHabitCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CoreConstants.MaxHabitCount);

        var sut = CreateSut(repository.Object);

        var act = () => sut.CreateHabitAsync(new HabitCreateInputModel { Name = "Workout" });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"*up to {CoreConstants.MaxHabitCount} habits*");
    }

    [Fact]
    public async Task CreateHabit_ShouldRejectDuplicateName_CaseInsensitive()
    {
        var repository = new Mock<ICoreRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetHabitByNameAsync(
                It.Is<string>(name => string.Equals(name, "reading", StringComparison.OrdinalIgnoreCase)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Habit { Id = "existing", Name = "reading", CreatedAtUtc = DateTime.UtcNow.ToString("O") });

        var sut = CreateSut(repository.Object);

        var act = () => sut.CreateHabitAsync(new HabitCreateInputModel { Name = " ReAdInG " });

        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task ToggleTodayCheckin_ShouldCreateDoneCheckinWhenMissing()
    {
        var habit = new Habit { Id = "habit-1", Name = "Read", SortOrder = 0, CreatedAtUtc = DateTime.UtcNow.ToString("O") };
        Checkin? captured = null;
        var repository = new Mock<ICoreRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetHabitByIdAsync(habit.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(habit);
        repository.Setup(x => x.GetCheckinAsync(habit.Id, TodayKey(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Checkin?)null);
        repository.Setup(x => x.UpsertCheckinAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()))
            .Callback<Checkin, CancellationToken>((checkin, _) => captured = checkin)
            .Returns(Task.CompletedTask);
        repository.Setup(x => x.GetCheckinsForHabitsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CreateSut(repository.Object);

        await sut.ToggleTodayCheckinAsync(new HabitToggleCheckinInputModel { HabitId = habit.Id });

        captured.Should().NotBeNull();
        captured!.CheckinDate.Should().Be(TodayKey());
        captured.IsDone.Should().Be(1);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    public async Task ToggleTodayCheckin_ShouldFlipExistingStatus(int existingValue, int expectedValue)
    {
        var habit = new Habit { Id = "habit-1", Name = "Read", SortOrder = 0, CreatedAtUtc = DateTime.UtcNow.ToString("O") };
        var existingCheckin = new Checkin
        {
            Id = "checkin-1",
            HabitId = habit.Id,
            CheckinDate = TodayKey(),
            IsDone = existingValue,
            CreatedAtUtc = DateTime.UtcNow.ToString("O")
        };

        Checkin? captured = null;
        var repository = new Mock<ICoreRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetHabitByIdAsync(habit.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(habit);
        repository.Setup(x => x.GetCheckinAsync(habit.Id, TodayKey(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCheckin);
        repository.Setup(x => x.UpsertCheckinAsync(It.IsAny<Checkin>(), It.IsAny<CancellationToken>()))
            .Callback<Checkin, CancellationToken>((checkin, _) => captured = checkin)
            .Returns(Task.CompletedTask);
        repository.Setup(x => x.GetCheckinsForHabitsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var sut = CreateSut(repository.Object);

        await sut.ToggleTodayCheckinAsync(new HabitToggleCheckinInputModel { HabitId = habit.Id });

        captured.Should().NotBeNull();
        captured!.IsDone.Should().Be(expectedValue);
    }

    [Theory]
    [MemberData(nameof(StreakCases))]
    public async Task StreakComputation_ShouldHandleKeyCases(int[] doneDayOffsets, int expectedStreak)
    {
        var habit = new Habit { Id = "habit-1", Name = "Read", SortOrder = 0, CreatedAtUtc = DateTime.UtcNow.ToString("O") };
        var checkins = doneDayOffsets
            .Select(offset => CreateCheckin(habit.Id, DateOnly.FromDateTime(DateTime.Today).AddDays(offset), 1))
            .ToList();

        var repository = new Mock<ICoreRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetHabitsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([habit]);
        repository.Setup(x => x.GetCheckinsForHabitsAsync(
                It.IsAny<IReadOnlyCollection<string>>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkins);

        var sut = CreateSut(repository.Object);

        var result = await sut.GetHabitsAsync();

        result.Should().ContainSingle();
        result[0].CurrentStreak.Should().Be(expectedStreak);
    }

    [Fact]
    public async Task ReminderSettingsUpdateValidation_ShouldRejectOutOfRangeTime()
    {
        var validator = new ReminderSettingsUpdateInputModelValidator();

        var result = await validator.ValidateAsync(new ReminderSettingsUpdateInputModel
        {
            ReminderTimeLocal = TimeSpan.FromDays(1)
        });

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x => x.ErrorMessage == "Reminder time must be between 00:00:00 and 23:59:59.");
    }

    [Fact]
    public async Task ReminderSettingsUpdate_ShouldPersistValidRange()
    {
        var settings = new AppSetting
        {
            Id = CoreConstants.ReminderSettingsId,
            IsReminderEnabled = 0,
            ReminderTimeLocal = TimeSpan.FromHours(21),
            UpdatedAtUtc = DateTime.UtcNow
        };

        AppSetting? captured = null;
        var repository = new Mock<ICoreRepository>(MockBehavior.Strict);
        repository.Setup(x => x.GetReminderSettingsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        repository.Setup(x => x.UpdateReminderSettingsAsync(It.IsAny<AppSetting>(), It.IsAny<CancellationToken>()))
            .Callback<AppSetting, CancellationToken>((value, _) => captured = value)
            .Returns(Task.CompletedTask);

        var sut = CreateSut(repository.Object);
        var input = new ReminderSettingsUpdateInputModel
        {
            IsReminderEnabled = true,
            ReminderTimeLocal = new TimeSpan(23, 59, 59)
        };

        var result = await sut.UpdateReminderSettingsAsync(input);

        captured.Should().NotBeNull();
        captured!.IsReminderEnabled.Should().Be(1);
        captured.ReminderTimeLocal.Should().Be(new TimeSpan(23, 59, 59));
        result.IsReminderEnabled.Should().BeTrue();
        result.ReminderTimeLocal.Should().Be(new TimeSpan(23, 59, 59));
    }

    public static IEnumerable<object[]> StreakCases()
    {
        yield return [Array.Empty<int>(), 0];
        yield return [new[] { 0, -1 }, 2];
        yield return [new[] { -2 }, 0];
    }

    private static CoreAppService CreateSut(ICoreRepository repository)
    {
        var mapper = new Mock<IMapper>(MockBehavior.Strict);
        mapper.Setup(x => x.Map<HabitViewModel>(It.IsAny<Habit>()))
            .Returns((Habit source) => new HabitViewModel
            {
                Id = source.Id,
                Name = source.Name,
                Emoji = source.Emoji,
                SortOrder = source.SortOrder
            });

        mapper.Setup(x => x.Map<ReminderSettingsViewModel>(It.IsAny<AppSetting>()))
            .Returns((AppSetting source) => new ReminderSettingsViewModel
            {
                IsReminderEnabled = source.IsReminderEnabled == 1,
                ReminderTimeLocal = source.ReminderTimeLocal
            });

        return new CoreAppService(
            repository,
            mapper.Object,
            new HabitCreateInputModelValidator(),
            new HabitUpdateInputModelValidator(),
            new HabitOrderUpdateInputModelValidator(),
            new HabitToggleCheckinInputModelValidator(),
            new HabitTrendQueryInputModelValidator(),
            new ReminderSettingsUpdateInputModelValidator());
    }

    private static Checkin CreateCheckin(string habitId, DateOnly date, int isDone)
    {
        return new Checkin
        {
            Id = Guid.NewGuid().ToString("N"),
            HabitId = habitId,
            CheckinDate = date.ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture),
            IsDone = isDone,
            CreatedAtUtc = DateTime.UtcNow.ToString("O")
        };
    }

    private static string TodayKey()
    {
        return DateOnly.FromDateTime(DateTime.Today).ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture);
    }
}