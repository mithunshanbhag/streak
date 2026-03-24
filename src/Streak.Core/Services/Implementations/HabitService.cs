namespace Streak.Core.Services.Implementations;

public sealed class HabitService(IHabitRepository habitRepository) : StreakServiceBase, IHabitService
{
    private readonly IHabitRepository _habitRepository = RequireNotNull(habitRepository, nameof(habitRepository));

    public async Task<IReadOnlyList<Habit>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var habits = await _habitRepository.GetAllAsync(cancellationToken);

        return
        [
            .. habits
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
        ];
    }

    public Task<Habit?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateHabitId(id, nameof(id));
        return _habitRepository.GetAsync(id, cancellationToken);
    }

    public async Task<Habit?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var normalizedName = NormalizeAndValidateHabitName(name, nameof(name));
        var habits = await _habitRepository.GetAllAsync(cancellationToken);

        return habits.FirstOrDefault(x => string.Equals(x.Name, normalizedName, StringComparison.OrdinalIgnoreCase));
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return _habitRepository.GetCountAsync(cancellationToken);
    }

    public async Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        var normalizedHabit = NormalizeHabitForWrite(habit, nameof(habit));
        var existingHabits = await _habitRepository.GetAllAsync(cancellationToken);

        if (existingHabits.Count >= CoreConstants.MaxHabitCount)
            throw new InvalidOperationException($"Cannot create more than {CoreConstants.MaxHabitCount} habits.");

        EnsureHabitNameIsUnique(normalizedHabit.Name, existingHabits);

        var nextId = existingHabits.Count == 0 ? 1 : existingHabits.Max(x => x.Id) + 1;

        var createdHabit = new Habit
        {
            Id = nextId,
            Name = normalizedHabit.Name,
            Emoji = normalizedHabit.Emoji
        };

        var isCreated = await _habitRepository.AddAsync(createdHabit, cancellationToken);
        if (!isCreated) throw new InvalidOperationException("Failed to create habit.");

        return createdHabit;
    }

    public async Task<Habit> UpdateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        var normalizedHabit = NormalizeHabitForWrite(habit, nameof(habit));
        ValidateHabitId(normalizedHabit.Id, nameof(Habit.Id));

        var existingHabit = await _habitRepository.GetAsync(normalizedHabit.Id, cancellationToken)
                            ?? throw new KeyNotFoundException($"Habit with id '{normalizedHabit.Id}' was not found.");

        var existingHabits = await _habitRepository.GetAllAsync(cancellationToken);

        EnsureHabitNameIsUnique(normalizedHabit.Name, existingHabits, normalizedHabit.Id);

        existingHabit.Name = normalizedHabit.Name;
        existingHabit.Emoji = normalizedHabit.Emoji;

        var isUpdated = await _habitRepository.UpdateAsync(existingHabit, cancellationToken);
        if (!isUpdated) throw new InvalidOperationException($"Failed to update habit with id '{existingHabit.Id}'.");

        return existingHabit;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        ValidateHabitId(id, nameof(id));

        var exists = await _habitRepository.ExistsAsync(id, cancellationToken);
        if (!exists) throw new KeyNotFoundException($"Habit with id '{id}' was not found.");

        var isDeleted = await _habitRepository.DeleteAsync(id, cancellationToken);
        if (!isDeleted) throw new InvalidOperationException($"Failed to delete habit with id '{id}'.");
    }

    private static Habit NormalizeHabitForWrite(Habit habit, string paramName)
    {
        var nonNullHabit = RequireNotNull(habit, paramName);
        var normalizedEmoji = NormalizeOptionalText(nonNullHabit.Emoji);

        if (!EmojiValidationHelper.IsEmptyOrSingleEmoji(normalizedEmoji))
            throw new ArgumentException("Emoji must be a single emoji.", nameof(Habit.Emoji));

        return new Habit
        {
            Id = nonNullHabit.Id,
            Name = NormalizeAndValidateHabitName(nonNullHabit.Name, nameof(Habit.Name)),
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

    private static void ValidateHabitId(int id, string paramName)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(paramName, "Habit ID must be greater than zero.");
    }

    private static void EnsureHabitNameIsUnique(
        string name,
        IReadOnlyList<Habit> existingHabits,
        int? excludedHabitId = null)
    {
        var duplicateExists = existingHabits.Any(x =>
            (!excludedHabitId.HasValue || x.Id != excludedHabitId.Value) &&
            string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

        if (duplicateExists) throw new InvalidOperationException($"A habit named '{name}' already exists.");
    }
}