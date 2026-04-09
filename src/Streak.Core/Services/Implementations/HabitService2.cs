namespace Streak.Core.Services.Implementations;

public sealed class HabitService2(IHabitRepository2 habitRepository)
    : StreakServiceBase, IHabitService2
{
    private readonly IHabitRepository2 _habitRepository =
        RequireNotNull(habitRepository, nameof(habitRepository));

    public async Task<IReadOnlyList<Habit2>> GetAllAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        var normalizedOwnerId = NormalizeRequiredText(ownerId, nameof(ownerId));
        var habits = await _habitRepository.ListByPartitionAsync(normalizedOwnerId, cancellationToken: cancellationToken);

        return
        [
            .. habits
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
        ];
    }

    public async Task<Habit2?> GetByIdAsync(
        string ownerId,
        string id,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedOwnerId = NormalizeRequiredText(ownerId, nameof(ownerId));
        var normalizedId = NormalizeRequiredText(id, nameof(id));

        var habit = await _habitRepository.GetAsync(normalizedOwnerId, normalizedId, cancellationToken);
        if (habit is not null || !throwIfNotExists) return habit;

        throw new KeyNotFoundException(
            $"Habit '{normalizedId}' for owner '{normalizedOwnerId}' was not found.");
    }

    public async Task<Habit2?> GetByNameAsync(
        string ownerId,
        string name,
        bool throwIfNotExists = true,
        CancellationToken cancellationToken = default)
    {
        var normalizedOwnerId = NormalizeRequiredText(ownerId, nameof(ownerId));
        var normalizedName = NormalizeAndValidateHabitName(name, nameof(name));
        var habits = await GetAllAsync(normalizedOwnerId, cancellationToken);

        var habit = habits.FirstOrDefault(x =>
            string.Equals(x.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

        if (habit is not null || !throwIfNotExists) return habit;

        throw new KeyNotFoundException(
            $"Habit '{normalizedName}' for owner '{normalizedOwnerId}' was not found.");
    }

    public async Task<int> GetCountAsync(string ownerId, CancellationToken cancellationToken = default)
    {
        var normalizedOwnerId = NormalizeRequiredText(ownerId, nameof(ownerId));
        var count = await _habitRepository.CountByPartitionAsync(
            normalizedOwnerId,
            cancellationToken: cancellationToken);

        if (count > int.MaxValue)
            throw new InvalidOperationException(
                $"Habit count '{count}' exceeds the supported range of '{int.MaxValue}'.");

        return (int)count;
    }

    public async Task<Habit2> CreateAsync(Habit2 habit, CancellationToken cancellationToken = default)
    {
        var normalizedHabit = NormalizeHabit(habit, nameof(habit), allowEmptyId: true);
        var existingHabits = await GetAllAsync(normalizedHabit.OwnerId, cancellationToken);

        if (existingHabits.Count >= CoreConstants.MaxHabitCount)
            throw new InvalidOperationException(
                $"Cannot create more than {CoreConstants.MaxHabitCount} habits for owner '{normalizedHabit.OwnerId}'.");

        EnsureHabitNameIsUnique(normalizedHabit.Name, existingHabits);

        var habitId = string.IsNullOrEmpty(normalizedHabit.Id)
            ? Guid.NewGuid().ToString("N")
            : normalizedHabit.Id;

        var existingHabit = await _habitRepository.GetAsync(
            normalizedHabit.OwnerId,
            habitId,
            cancellationToken);

        if (existingHabit is not null)
            throw new InvalidOperationException(
                $"A habit with id '{habitId}' already exists for owner '{normalizedHabit.OwnerId}'.");

        var createdHabit = new Habit2
        {
            Id = habitId,
            OwnerId = normalizedHabit.OwnerId,
            Name = normalizedHabit.Name,
            Emoji = normalizedHabit.Emoji
        };

        await _habitRepository.AddAsync(createdHabit.OwnerId, createdHabit, cancellationToken);
        return createdHabit;
    }

    public async Task<Habit2> UpdateAsync(Habit2 habit, CancellationToken cancellationToken = default)
    {
        var normalizedHabit = NormalizeHabit(habit, nameof(habit));
        var existingHabit = await _habitRepository.GetAsync(
            normalizedHabit.OwnerId,
            normalizedHabit.Id,
            cancellationToken);

        if (existingHabit is null)
            throw new KeyNotFoundException(
                $"Habit '{normalizedHabit.Id}' for owner '{normalizedHabit.OwnerId}' was not found.");

        var existingHabits = await GetAllAsync(normalizedHabit.OwnerId, cancellationToken);
        EnsureHabitNameIsUnique(normalizedHabit.Name, existingHabits, normalizedHabit.Id);

        var updatedHabit = new Habit2
        {
            Id = existingHabit.Id,
            OwnerId = existingHabit.OwnerId,
            Name = normalizedHabit.Name,
            Emoji = normalizedHabit.Emoji
        };

        await _habitRepository.UpsertAsync(updatedHabit.OwnerId, updatedHabit, cancellationToken);
        return updatedHabit;
    }

    public async Task DeleteAsync(string ownerId, string id, CancellationToken cancellationToken = default)
    {
        var normalizedOwnerId = NormalizeRequiredText(ownerId, nameof(ownerId));
        var normalizedId = NormalizeRequiredText(id, nameof(id));
        var existingHabit = await _habitRepository.GetAsync(
            normalizedOwnerId,
            normalizedId,
            cancellationToken);

        if (existingHabit is null)
            throw new KeyNotFoundException(
                $"Habit '{normalizedId}' for owner '{normalizedOwnerId}' was not found.");

        var deleted = await _habitRepository.DeleteIfExistsAsync(
            normalizedOwnerId,
            normalizedId,
            cancellationToken);

        if (!deleted)
            throw new InvalidOperationException(
                $"Failed to delete habit '{normalizedId}' for owner '{normalizedOwnerId}'.");
    }

    #region Private Helper Methods

    private static Habit2 NormalizeHabit(Habit2 habit, string paramName, bool allowEmptyId = false)
    {
        var nonNullHabit = RequireNotNull(habit, paramName);
        var normalizedId = NormalizeOptionalText(nonNullHabit.Id);
        var normalizedEmoji = NormalizeOptionalText(nonNullHabit.Emoji);

        if (!allowEmptyId && normalizedId is null)
            throw new ArgumentException("Habit ID cannot be null or whitespace.", nameof(Habit2.Id));

        if (!EmojiValidationHelper.IsEmptyOrSingleEmoji(normalizedEmoji))
            throw new ArgumentException("Emoji must be a single emoji.", nameof(Habit2.Emoji));

        return new Habit2
        {
            Id = normalizedId ?? string.Empty,
            OwnerId = NormalizeRequiredText(nonNullHabit.OwnerId, nameof(Habit2.OwnerId)),
            Name = NormalizeAndValidateHabitName(nonNullHabit.Name, nameof(Habit2.Name)),
            Emoji = normalizedEmoji
        };
    }

    private static string NormalizeAndValidateHabitName(string name, string paramName)
    {
        var normalizedName = NormalizeRequiredText(name, paramName);
        if (normalizedName.Length is < CoreConstants.HabitNameMinLength or > CoreConstants.HabitNameMaxLength)
            throw new ArgumentException(
                $"Habit name must be between {CoreConstants.HabitNameMinLength} and {CoreConstants.HabitNameMaxLength} characters.",
                paramName);

        return normalizedName;
    }

    private static void EnsureHabitNameIsUnique(
        string name,
        IReadOnlyList<Habit2> existingHabits,
        string? excludedHabitId = null)
    {
        var duplicateExists = existingHabits.Any(x =>
            (excludedHabitId is null || x.Id != excludedHabitId) &&
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (duplicateExists)
            throw new InvalidOperationException($"A habit named '{name}' already exists.");
    }

    #endregion
}
