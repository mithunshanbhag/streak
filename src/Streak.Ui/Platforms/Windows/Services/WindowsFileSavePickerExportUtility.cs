#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Streak.Ui.Services.Implementations;

internal static class WindowsFileSavePickerExportUtility
{
    public static async Task<bool> SaveFileAsync(
        string sourceFilePath,
        string fileTypeDescription,
        string defaultFileExtension,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileTypeDescription);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultFileExtension);

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("The generated export file could not be found.", sourceFilePath);

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = Path.GetFileNameWithoutExtension(sourceFilePath),
            DefaultFileExtension = defaultFileExtension
        };

        picker.FileTypeChoices.Add(fileTypeDescription, [defaultFileExtension]);

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var targetFile = await picker.PickSaveFileAsync();
        if (targetFile is null)
            return false;

        await using var sourceStream = File.OpenRead(sourceFilePath);
        await using var destinationStream = await targetFile.OpenStreamForWriteAsync();

        destinationStream.SetLength(0);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        await destinationStream.FlushAsync(cancellationToken);

        return true;
    }

    private static nint GetWindowHandle()
    {
        var nativeWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                           ?? throw new InvalidOperationException("A native Windows window is required to save exports.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif
