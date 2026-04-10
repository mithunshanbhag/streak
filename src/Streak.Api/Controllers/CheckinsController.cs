namespace Streak.Api.Controllers;

public sealed class CheckinsController(
    IHabitService2 habitService,
    ICheckinService2 checkinService,
    IMediator mediator,
    ILogger<NControllerBase> logger)
    : NControllerBase(mediator, logger)
{
    private readonly IHabitService2 _habitService = habitService;
    private readonly ICheckinService2 _checkinService = checkinService;

    [Function(nameof(GetCheckinHistoryAsync))]
    public Task<IActionResult> GetCheckinHistoryAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = ApiConstants.OwnerHabitCheckinsRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            await _habitService.GetByIdAsync(ownerId, habitId, cancellationToken: cancellationToken);

            var fromDate = request.Query["fromDate"].ToString();
            var toDate = request.Query["toDate"].ToString();

            var checkins = await _checkinService.GetHistoryAsync(
                habitId,
                string.IsNullOrWhiteSpace(fromDate) ? null : fromDate,
                string.IsNullOrWhiteSpace(toDate) ? null : toDate,
                cancellationToken);

            var response = checkins
                .Select(MapCheckinResponse)
                .ToArray();

            return new OkObjectResult(response);
        });
    }

    [Function(nameof(ToggleTodayCheckinAsync))]
    public Task<IActionResult> ToggleTodayCheckinAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = ApiConstants.OwnerHabitCheckinTodayRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            var payload = await ReadRequiredBodyAsync<ToggleTodayCheckinRequestDto>(request, cancellationToken);
            var checkin = await _checkinService.ToggleForTodayAsync(
                ownerId,
                habitId,
                payload.IsDone,
                cancellationToken);

            return checkin is null
                ? new NoContentResult()
                : new OkObjectResult(MapCheckinResponse(checkin));
        });
    }

    [Function(nameof(DeleteCheckinAsync))]
    public Task<IActionResult> DeleteCheckinAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = ApiConstants.OwnerHabitCheckinRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        string checkinDate,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            await _checkinService.DeleteForHabitAndDateAsync(ownerId, habitId, checkinDate, cancellationToken);
            return new NoContentResult();
        });
    }

    [Function(nameof(GetCurrentStreakAsync))]
    public Task<IActionResult> GetCurrentStreakAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = ApiConstants.OwnerHabitStreakRoute)]
        HttpRequest request,
        string ownerId,
        string habitId,
        CancellationToken cancellationToken)
    {
        return ProcessAsync(async () =>
        {
            await _habitService.GetByIdAsync(ownerId, habitId, cancellationToken: cancellationToken);
            var currentStreak = await _checkinService.GetCurrentStreakAsync(habitId, cancellationToken);

            return new OkObjectResult(new CurrentStreakResponseDto
            {
                HabitId = habitId,
                CurrentStreak = currentStreak
            });
        });
    }

    private static CheckinResponseDto MapCheckinResponse(Checkin2 checkin)
    {
        return new CheckinResponseDto
        {
            Id = checkin.Id,
            HabitId = checkin.HabitId,
            OwnerId = checkin.OwnerId,
            CheckinDate = checkin.CheckinDate
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
