#if WINDOWS
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Streak.Ui.Services.Implementations;

public sealed class WindowsDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("The generated backup file could not be found.", backupFilePath);

        var picker = new FileSavePicker
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            SuggestedFileName = Path.GetFileNameWithoutExtension(backupFilePath),
            DefaultFileExtension = ".db"
        };

        picker.FileTypeChoices.Add("SQLite database backup", [".db"]);

        InitializeWithWindow.Initialize(picker, GetWindowHandle());

        var targetFile = await picker.PickSaveFileAsync();
        if (targetFile is null)
            return DatabaseExportResult.Cancelled;

        await using var sourceStream = File.OpenRead(backupFilePath);
        await using var destinationStream = await targetFile.OpenStreamForWriteAsync();

        destinationStream.SetLength(0);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
        await destinationStream.FlushAsync(cancellationToken);

        return DatabaseExportResult.Saved;
    }

    private static nint GetWindowHandle()
    {
        var nativeWindow = Microsoft.Maui.Controls.Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window
                           ?? throw new InvalidOperationException("A native Windows window is required to save exports.");

        return WindowNative.GetWindowHandle(nativeWindow);
    }
}
#endif