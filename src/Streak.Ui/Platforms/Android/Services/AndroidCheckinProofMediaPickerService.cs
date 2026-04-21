#if ANDROID
namespace Streak.Ui.Services.Implementations;

public sealed class AndroidCheckinProofMediaPickerService : ICheckinProofMediaPickerService
{
    private const string CapturePhotoTitleText = "Capture a picture proof";
    private const string PickPhotoTitleText = "Choose a picture proof";

    private static readonly PickOptions PickOptions = new()
    {
        PickerTitle = PickPhotoTitleText,
        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.Android] = ["image/*"]
        })
    };

    public bool SupportsCameraCapture => MediaPicker.Default.IsCaptureSupported;

    public async Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await MediaPicker.Default.CapturePhotoAsync(
            new MediaPickerOptions
            {
                Title = CapturePhotoTitleText
            });
    }

    public async Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await FilePicker.Default.PickAsync(PickOptions);
    }
}
#endif
