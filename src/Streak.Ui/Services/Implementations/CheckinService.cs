namespace Streak.Ui.Services.Implementations;

public class CheckinService(
    ICheckinRepository checkinRepository,
    IHabitRepository habitRepository,
    ILogger<CheckinService> logger,
    TimeProvider timeProvider)
    : StreakServiceBase, ICheckinService
{
    private const string DateFormat = "yyyy-MM-dd";

    private readonly ICheckinRepository _checkinRepository =
        RequireNotNull(checkinRepository, nameof(checkinRepository));

    private readonly IHabitRepository _habitRepository =
        RequireNotNull(habitRepository, nameof(habitRepository));

    private readonly ILogger<CheckinService> _logger =
        RequireNotNull(logger, nameof(logger));

    private readonly TimeProvider _timeProvider =
        RequireNotNull(timeProvider, nameof(timeProvider));

    public async Task<Checkin?> GetByHabitNameAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));

        return await _checkinRepository.GetByHabitNameAndDateAsync(
            normalizedHabitName,
            normalizedCheckinDate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Checkin>> GetHistoryAsync(
        string habitName,
        string? fromDate = null,
        string? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var normalizedFromDate = NormalizeOptionalDate(fromDate, nameof(fromDate));
        var normalizedToDate = NormalizeOptionalDate(toDate, nameof(toDate));

        if (normalizedFromDate is not null &&
            normalizedToDate is not null &&
            string.CompareOrdinal(normalizedFromDate, normalizedToDate) > 0)
            throw new ArgumentException("'fromDate' cannot be greater than 'toDate'.", nameof(fromDate));

        return await _checkinRepository.GetByHabitNamesAsync(
            [normalizedHabitName],
            normalizedFromDate,
            normalizedToDate,
            cancellationToken);
    }

    public async Task<IReadOnlyList<HabitCheckinViewModel>> GetHomePageHabitCheckinsAsync(
        CancellationToken cancellationToken = default)
    {
        var totalStopwatch = Stopwatch.StartNew();
        var stepStopwatch = Stopwatch.StartNew();

        var habits = await _habitRepository.GetAllAsync(cancellationToken);

        _logger.LogInformation(
            "Home page habit query completed in {ElapsedMilliseconds} ms with {HabitCount} habit(s).",
            stepStopwatch.ElapsedMilliseconds,
            habits.Count);

        if (habits.Count == 0)
        {
            _logger.LogInformation(
                "Home page check-in service load completed in {ElapsedMilliseconds} ms with {HabitCount} habit(s), {CheckinKeyCount} check-in key row(s), and {ViewModelCount} card(s).",
                totalStopwatch.ElapsedMilliseconds,
                habits.Count,
                0,
                0);

            return [];
        }

        var orderedHabits =
            habits.OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        var todayLocal = GetTodayLocalDate();

        stepStopwatch.Restart();
        var checkinKeys = await _checkinRepository.GetKeysByHabitIdsAsync(
            [.. orderedHabits.Select(x => x.Id)],
            toDate: FormatDate(todayLocal),
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Home page check-in key query completed in {ElapsedMilliseconds} ms with {CheckinKeyCount} check-in key row(s) for {HabitCount} habit(s).",
            stepStopwatch.ElapsedMilliseconds,
            checkinKeys.Count,
            orderedHabits.Length);

        stepStopwatch.Restart();
        var viewModels = BuildHomePageHabitCheckinViewModels(orderedHabits, checkinKeys, todayLocal);

        _logger.LogInformation(
            "Home page view model build completed in {ElapsedMilliseconds} ms with {ViewModelCount} card(s).",
            stepStopwatch.ElapsedMilliseconds,
            viewModels.Count);

        _logger.LogInformation(
            "Home page check-in service load completed in {ElapsedMilliseconds} ms with {HabitCount} habit(s), {CheckinKeyCount} check-in key row(s), and {ViewModelCount} card(s).",
            totalStopwatch.ElapsedMilliseconds,
            orderedHabits.Length,
            checkinKeys.Count,
            viewModels.Count);

        return viewModels;
    }

    public async Task<int> GetPendingHabitCountForTodayAsync(CancellationToken cancellationToken = default)
    {
        var habits = await _habitRepository.GetAllAsync(cancellationToken);
        if (habits.Count == 0) return 0;

        var todayLocal = GetTodayLocalDate();
        var todayLocalString = FormatDate(todayLocal);
        var todayCheckins = await _checkinRepository.GetKeysByHabitIdsAsync(
            [.. habits.Select(x => x.Id)],
            fromDate: todayLocalString,
            toDate: todayLocalString,
            cancellationToken: cancellationToken);

        var completedHabitCount = todayCheckins
            .Select(x => x.HabitId)
            .Distinct()
            .Count();

        return Math.Max(0, habits.Count - completedHabitCount);
    }

    public async Task<Checkin> UpsertAsync(Checkin checkin, CancellationToken cancellationToken = default)
    {
        var normalizedCheckin = NormalizeRequiredCheckin(checkin);

        await EnsureHabitExistsAsync(normalizedCheckin.HabitId, cancellationToken);

        var existingCheckin = await _checkinRepository.GetAsync(
            new CheckinKey(normalizedCheckin.HabitId, normalizedCheckin.CheckinDate),
            cancellationToken);

        if (existingCheckin is null)
        {
            var added = await _checkinRepository.AddAsync(normalizedCheckin, cancellationToken);
            if (!added)
                throw new InvalidOperationException(
                    $"Unable to add checkin for habit id '{normalizedCheckin.HabitId}' on '{normalizedCheckin.CheckinDate}'.");

            return normalizedCheckin;
        }

        return existingCheckin;
    }

    public Task<Checkin?> ToggleForTodayAsync(
        string habitName,
        bool isDone,
        string? notes = null,
        CheckinProofInputModel? proof = null,
        CancellationToken cancellationToken = default)
    {
        return ToggleForTodayInternalAsync(habitName, isDone, notes, proof, cancellationToken);
    }

    public async Task DeleteForHabitAndDateAsync(
        string habitName,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        var habit = await GetRequiredHabitByNameAsync(habitName, cancellationToken);
        var normalizedCheckinDate = NormalizeRequiredDate(checkinDate, nameof(checkinDate));

        var checkinKey = new CheckinKey(habit.Id, normalizedCheckinDate);
        var checkinExists = await _checkinRepository.ExistsAsync(checkinKey, cancellationToken);
        if (!checkinExists)
            throw new InvalidOperationException(
                $"Checkin for habit '{habit.Name}' on '{normalizedCheckinDate}' does not exist.");

        var deleted = await _checkinRepository.DeleteAsync(checkinKey, cancellationToken);

        if (!deleted)
            throw new InvalidOperationException(
                $"Unable to delete checkin for habit '{habit.Name}' on '{normalizedCheckinDate}'.");
    }

    public async Task<int> GetCurrentStreakAsync(string habitName, CancellationToken cancellationToken = default)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));
        var checkinHistory = await _checkinRepository.GetByHabitNameAsync(normalizedHabitName, cancellationToken);
        return CalculateCurrentStreak(
            BuildCheckinDateSet(checkinHistory),
            GetTodayLocalDate());
    }

    private async Task<Checkin?> ToggleForTodayInternalAsync(
        string habitName,
        bool isDone,
        string? notes,
        CheckinProofInputModel? proof,
        CancellationToken cancellationToken)
    {
        var habit = await GetRequiredHabitByNameAsync(habitName, cancellationToken);
        var todayDate = GetTodayLocalDateString();

        if (!isDone)
        {
            await _checkinRepository.DeleteAsync(new CheckinKey(habit.Id, todayDate), cancellationToken);
            return null;
        }

        var checkin = new Checkin
        {
            HabitId = habit.Id,
            CheckinDate = todayDate,
            Notes = notes,
            ProofImageUri = proof?.ProofImageUri,
            ProofImageDisplayName = proof?.ProofImageDisplayName,
            ProofImageSizeBytes = proof?.ProofImageSizeBytes,
            ProofImageModifiedOn = proof?.ProofImageModifiedOn
        };

        return await UpsertAsync(checkin, cancellationToken);
    }

    private async Task<Habit> GetRequiredHabitByNameAsync(string habitName, CancellationToken cancellationToken)
    {
        var normalizedHabitName = NormalizeRequiredText(habitName, nameof(habitName));

        return await _habitRepository.GetByNameAsync(normalizedHabitName, cancellationToken)
               ?? throw new InvalidOperationException($"Habit '{normalizedHabitName}' does not exist.");
    }

    private async Task EnsureHabitExistsAsync(int habitId, CancellationToken cancellationToken)
    {
        ValidateHabitId(habitId, nameof(habitId));

        var habitExists = await _habitRepository.ExistsAsync(habitId, cancellationToken);
        if (!habitExists) throw new InvalidOperationException($"Habit id '{habitId}' does not exist.");
    }

    private static IReadOnlyList<HabitCheckinViewModel> BuildHomePageHabitCheckinViewModels(
        IReadOnlyList<Habit> habits,
        IReadOnlyList<CheckinKey> checkins,
        DateOnly todayLocal)
    {
        var checkinDatesByHabitId = checkins
            .GroupBy(x => x.HabitId)
            .ToDictionary(
                x => x.Key,
                x => BuildCheckinDateSet(x));

        return
        [
            .. habits.Select(habit =>
            {
                checkinDatesByHabitId.TryGetValue(habit.Id, out var checkinDates);
                return CreateHomePageHabitCheckinViewModel(habit, checkinDates, todayLocal);
            })
        ];
    }

    private static HabitCheckinViewModel CreateHomePageHabitCheckinViewModel(
        Habit habit,
        HashSet<DateOnly>? checkinDates,
        DateOnly todayLocal)
    {
        var safeCheckinDates = checkinDates ?? [];

        return new HabitCheckinViewModel
        {
            HabitId = habit.Id,
            HabitName = habit.Name,
            HabitEmoji = habit.Emoji,
            Streak = CalculateCurrentStreak(safeCheckinDates, todayLocal),
            IsDoneForToday = safeCheckinDates.Contains(todayLocal)
        };
    }

    private static int CalculateCurrentStreak(
        HashSet<DateOnly> checkinDates,
        DateOnly todayLocal)
    {
        if (checkinDates.Count == 0) return 0;

        var streakStartDate = checkinDates.Contains(todayLocal)
            ? todayLocal
            : todayLocal.AddDays(-1);

        var streak = 0;
        var currentDate = streakStartDate;

        while (checkinDates.Contains(currentDate))
        {
            streak++;
            currentDate = currentDate.AddDays(-1);
        }

        return streak;
    }

    private static HashSet<DateOnly> BuildCheckinDateSet(IEnumerable<Checkin> checkins)
    {
        HashSet<DateOnly> checkinDates = [];

        foreach (var checkin in checkins)
        {
            if (!TryParseDate(checkin.CheckinDate, out var checkinDate)) continue;
            checkinDates.Add(checkinDate);
        }

        return checkinDates;
    }

    private static HashSet<DateOnly> BuildCheckinDateSet(IEnumerable<CheckinKey> checkinKeys)
    {
        HashSet<DateOnly> checkinDates = [];

        foreach (var checkinKey in checkinKeys)
        {
            if (!TryParseDate(checkinKey.CheckinDate, out var checkinDate)) continue;
            checkinDates.Add(checkinDate);
        }

        return checkinDates;
    }

    private static Checkin NormalizeRequiredCheckin(Checkin checkin)
    {
        RequireNotNull(checkin, nameof(checkin));

        var normalizedCheckinDate = NormalizeRequiredDate(checkin.CheckinDate, nameof(checkin.CheckinDate));
        var normalizedNotes = NormalizeOptionalText(checkin.Notes);
        var normalizedProof = NormalizeOptionalCheckinProof(checkin);
        if (checkin.HabitId <= 0)
            throw new ArgumentOutOfRangeException(nameof(checkin.HabitId), "Habit ID must be greater than zero.");
        if (normalizedNotes?.Length > CoreConstants.CheckinNotesMaxLength)
            throw new ArgumentException(
                $"Checkin notes must be {CoreConstants.CheckinNotesMaxLength} characters or fewer.",
                nameof(checkin.Notes));

        return new Checkin
        {
            HabitId = checkin.HabitId,
            CheckinDate = normalizedCheckinDate,
            Notes = normalizedNotes,
            ProofImageUri = normalizedProof?.ProofImageUri,
            ProofImageDisplayName = normalizedProof?.ProofImageDisplayName,
            ProofImageSizeBytes = normalizedProof?.ProofImageSizeBytes,
            ProofImageModifiedOn = normalizedProof?.ProofImageModifiedOn
        };
    }

    private static CheckinProofInputModel? NormalizeOptionalCheckinProof(Checkin checkin)
    {
        var normalizedProofImageUri = NormalizeOptionalText(checkin.ProofImageUri);
        var normalizedProofImageDisplayName = NormalizeOptionalText(checkin.ProofImageDisplayName);
        var normalizedProofImageModifiedOn = NormalizeOptionalText(checkin.ProofImageModifiedOn);
        var proofImageSizeBytes = checkin.ProofImageSizeBytes;

        var hasAnyProofValue =
            normalizedProofImageUri is not null ||
            normalizedProofImageDisplayName is not null ||
            normalizedProofImageModifiedOn is not null ||
            proofImageSizeBytes is not null;

        if (!hasAnyProofValue)
            return null;

        if (normalizedProofImageUri is null)
            throw new ArgumentException("Proof image URI is required when proof metadata is provided.", nameof(checkin.ProofImageUri));

        if (normalizedProofImageDisplayName is null)
            throw new ArgumentException("Proof image display name is required when proof metadata is provided.", nameof(checkin.ProofImageDisplayName));

        if (normalizedProofImageModifiedOn is null)
            throw new ArgumentException("Proof image modified time is required when proof metadata is provided.", nameof(checkin.ProofImageModifiedOn));

        if (!proofImageSizeBytes.HasValue)
            throw new ArgumentException("Proof image size is required when proof metadata is provided.", nameof(checkin.ProofImageSizeBytes));

        if (proofImageSizeBytes.Value is <= 0 or > CoreConstants.CheckinProofMaxSizeBytes)
            throw new ArgumentOutOfRangeException(
                nameof(checkin.ProofImageSizeBytes),
                $"Proof image size must be between 1 and {CoreConstants.CheckinProofMaxSizeBytes} bytes.");

        if (!DateTimeOffset.TryParseExact(
                normalizedProofImageModifiedOn,
                CoreConstants.CheckinProofModifiedOnFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsedModifiedOn))
            throw new ArgumentException(
                $"Proof image modified time must match format '{CoreConstants.CheckinProofModifiedOnFormat}'.",
                nameof(checkin.ProofImageModifiedOn));

        return new CheckinProofInputModel
        {
            ProofImageUri = normalizedProofImageUri,
            ProofImageDisplayName = normalizedProofImageDisplayName,
            ProofImageSizeBytes = proofImageSizeBytes.Value,
            ProofImageModifiedOn = parsedModifiedOn.ToString(
                CoreConstants.CheckinProofModifiedOnFormat,
                CultureInfo.InvariantCulture)
        };
    }

    private static string NormalizeRequiredDate(string value, string paramName)
    {
        var normalizedDate = NormalizeRequiredText(value, paramName);
        if (!TryParseDate(normalizedDate, out var parsedDate)) throw new ArgumentException($"Date must match format '{DateFormat}'.", paramName);

        return parsedDate.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private static string? NormalizeOptionalDate(string? value, string paramName)
    {
        var normalizedDate = NormalizeOptionalText(value);
        if (normalizedDate is null) return null;

        if (!TryParseDate(normalizedDate, out var parsedDate)) throw new ArgumentException($"Date must match format '{DateFormat}'.", paramName);

        return parsedDate.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private DateOnly GetTodayLocalDate()
    {
        return DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
    }

    private string GetTodayLocalDateString()
    {
        return FormatDate(GetTodayLocalDate());
    }

    private static string FormatDate(DateOnly date)
    {
        return date.ToString(DateFormat, CultureInfo.InvariantCulture);
    }

    private static void ValidateHabitId(int habitId, string paramName)
    {
        if (habitId <= 0)
            throw new ArgumentOutOfRangeException(paramName, "Habit ID must be greater than zero.");
    }

    private static bool TryParseDate(string value, out DateOnly parsedDate)
    {
        return DateOnly.TryParseExact(
            value,
            DateFormat,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out parsedDate);
    }
}
