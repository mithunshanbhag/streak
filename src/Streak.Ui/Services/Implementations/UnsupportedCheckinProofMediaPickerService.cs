namespace Streak.Ui.Services.Implementations;

public sealed class UnsupportedCheckinProofMediaPickerService : ICheckinProofMediaPickerService
{
    public bool SupportsCameraCapture => false;

    public Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<FileResult?>(null);
    }

    public Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<FileResult?>(null);
    }
}
