namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedDiagnosticsShareService : IDiagnosticsShareService
{
    public bool CanShare => false;

    public Task ShareDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Diagnostics sharing is not supported on this platform.");
    }
}
