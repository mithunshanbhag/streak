#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Streak.Ui.Services.Implementations;

public sealed class WindowsDatabaseImportFilePicker : IDatabaseImportFilePicker
{
    public async Task<FileResult?> PickBackupAsync(CancellationToken cancellationToken = default)
    {
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

        return new FileResult(backupFile.Path);
    }

    private static nint GetWindowHandle()
    {
        var nativeWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                           ?? throw new InvalidOperationException("A native Windows window is required to select database backups.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif
