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

        picker.FileTypeFilter.Add(".zip");

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var backupFile = await picker.PickSingleFileAsync();
        if (backupFile is null)
            return null;

        cancellationToken.ThrowIfCancellationRequested();

        return new FileResult(backupFile.Path);
    }

    private static nint GetWindowHandle()
    {
        var currentApplication = Microsoft.Maui.Controls.Application.Current
                                 ?? throw new InvalidOperationException("Unable to initialize the file picker because the MAUI application instance is not available.");

        var activeWindow = currentApplication.Windows.FirstOrDefault()
                           ?? throw new InvalidOperationException("Unable to initialize the file picker because no active application window was found. Ensure restore is launched from an active UI window.");

        var nativeWindow = activeWindow.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                           ?? throw new InvalidOperationException("Unable to initialize the file picker because the native Windows window has not been created yet.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif
