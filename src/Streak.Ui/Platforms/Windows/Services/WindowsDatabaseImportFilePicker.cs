#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Streak.Ui.Services.Implementations;

public sealed class WindowsDatabaseImportFilePicker : IDatabaseImportFilePicker
{
    public async Task<FileResult?> PickBackupAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var picker = new FileOpenPicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List
        };

        picker.FileTypeFilter.Add(".db");

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var backupFile = await picker.PickSingleFileAsync();
        if (backupFile is null)
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        return new FileResult(backupFile.Path);
    }

    private static nint GetWindowHandle()
    {
        var nativeWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                           ?? throw new InvalidOperationException("Unable to initialize the file picker because no active application window was found. Ensure restore is launched from an active UI window.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif
