namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedDatabaseShareService : IDatabaseShareService
{
    public bool CanShare => false;

    public Task ShareDatabaseAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Database sharing is not supported on this platform.");
    }
}
