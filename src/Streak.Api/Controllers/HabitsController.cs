namespace Streak.Api.Controllers;

public sealed class HabitsController(
    IHabitService2 habitService,
    ICheckinService2 checkinService,
    IValidator<CreateHabitRequestDto> createHabitRequestValidator,
    IValidator<UpdateHabitRequestDto> updateHabitRequestValidator,
    IMediator mediator,
    ILogger<NControllerBase> logger)
    : NControllerBase(mediator, logger)
{
    private readonly IHabitService2 _habitService = habitService;
    private readonly ICheckinService2 _checkinService = checkinService;
    private readonly IValidator<CreateHabitRequestDto> _createHabitRequestValidator = createHabitRequestValidator;
    private readonly IValidator<UpdateHabitRequestDto> _updateHabitRequestValidator = updateHabitRequestValidator;

    [Function(nameof(GetHabitsAsync))]
    public Task<IActionResult> GetHabitsAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = ApiConstants.OwnerHabitsRoute)]
        HttpRequest request,
        string ownerId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            var habits = await _habitService.GetAllAsync(ownerId, cancellationToken);
            var habitResponseTasks = habits.Select(habit => MapHabitResponseAsync(habit, cancellationToken));
            var response = await Task.WhenAll(habitResponseTasks);

            return new OkObjectResult(response);
        });
    }

    [Function(nameof(GetHabitByIdAsync))]
    public Task<IActionResult> GetHabitByIdAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = ApiConstants.OwnerHabitRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            var habit = await _habitService.GetByIdAsync(ownerId, habitId, cancellationToken: cancellationToken);
            var response = await MapHabitResponseAsync(habit!, cancellationToken);

            return new OkObjectResult(response);
        });
    }

    [Function(nameof(CreateHabitAsync))]
    public Task<IActionResult> CreateHabitAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = ApiConstants.OwnerHabitsRoute)]
        HttpRequest request,
        string ownerId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            var payload = await ReadRequiredBodyAsync<CreateHabitRequestDto>(request, cancellationToken);
            await _createHabitRequestValidator.ValidateAndThrowAsync(payload, cancellationToken);

            var createdHabit = await _habitService.CreateAsync(
                new Habit2
                {
                    Id = string.Empty,
                    OwnerId = ownerId,
                    Name = payload.Name,
                    Emoji = payload.Emoji
                },
                cancellationToken);

            var response = await MapHabitResponseAsync(createdHabit, cancellationToken);
            return new CreatedResult(ApiConstants.GetHabitRoute(response.OwnerId, response.Id), response);
        });
    }

    [Function(nameof(UpdateHabitAsync))]
    public Task<IActionResult> UpdateHabitAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = ApiConstants.OwnerHabitRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            var payload = await ReadRequiredBodyAsync<UpdateHabitRequestDto>(request, cancellationToken);
            await _updateHabitRequestValidator.ValidateAndThrowAsync(payload, cancellationToken);

            var updatedHabit = await _habitService.UpdateAsync(
                new Habit2
                {
                    Id = habitId,
                    OwnerId = ownerId,
                    Name = payload.Name,
                    Emoji = payload.Emoji
                },
                cancellationToken);

            var response = await MapHabitResponseAsync(updatedHabit, cancellationToken);
            return new OkObjectResult(response);
        });
    }

    [Function(nameof(DeleteHabitAsync))]
    public Task<IActionResult> DeleteHabitAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = ApiConstants.OwnerHabitRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            await _habitService.DeleteAsync(ownerId, habitId, cancellationToken);
            return new NoContentResult();
        });
    }

    private async Task<HabitResponseDto> MapHabitResponseAsync(Habit2 habit, CancellationToken cancellationToken)
    {
        var currentStreak = await _checkinService.GetCurrentStreakAsync(habit.Id, cancellationToken);

        return new HabitResponseDto
        {
            Id = habit.Id,
            OwnerId = habit.OwnerId,
            Name = habit.Name,
            Emoji = habit.Emoji,
            CurrentStreak = currentStreak
        };
    }

    private static async Task<T> ReadRequiredBodyAsync<T>(
        HttpRequest request,
        CancellationToken cancellationToken)
        where T : class
    {
        var payload = await request.ReadFromJsonAsync<T>(cancellationToken);
        return payload ?? throw new ValidationException("Request body is required.");
    }
}
