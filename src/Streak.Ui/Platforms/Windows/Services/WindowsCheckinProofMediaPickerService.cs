#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Streak.Ui.Services.Implementations;

public sealed class WindowsCheckinProofMediaPickerService : ICheckinProofMediaPickerService
{
    public bool SupportsCameraCapture => false;

    public Task<FileResult?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<FileResult?>(null);
    }

    public async Task<FileResult?> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.PicturesLibrary,
            ViewMode = PickerViewMode.Thumbnail
        };

        picker.FileTypeFilter.Add(".jpg");
        picker.FileTypeFilter.Add(".jpeg");
        picker.FileTypeFilter.Add(".png");
        picker.FileTypeFilter.Add(".bmp");
        picker.FileTypeFilter.Add(".gif");
        picker.FileTypeFilter.Add(".webp");

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var selectedPhoto = await picker.PickSingleFileAsync();
        if (selectedPhoto is null)
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        return new FileResult(selectedPhoto.Path);
    }

    private static nint GetWindowHandle()
    {
        var currentApplication = Microsoft.Maui.Controls.Application.Current
                                 ?? throw new InvalidOperationException("Unable to initialize the proof picker because the MAUI application instance is not available.");

        var activeWindow = currentApplication.Windows.FirstOrDefault()
                           ?? throw new InvalidOperationException("Unable to initialize the proof picker because no active application window was found.");

        var nativeWindow = activeWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                           ?? throw new InvalidOperationException("Unable to initialize the proof picker because the native Windows window has not been created yet.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif
