namespace Streak.Ui.Services.Implementations;

public sealed class DatabaseImportFilePicker : IDatabaseImportFilePicker
{
    private static readonly PickOptions PickOptions = new()
    {
        PickerTitle = PickerTitleText,
        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.WinUI] = [".db"],
            [DevicePlatform.Android] = ["application/octet-stream", "application/x-sqlite3", "application/vnd.sqlite3"]
        })
    };

    public Task<FileResult?> PickBackupAsync(CancellationToken cancellationToken = default)
    {
        return FilePicker.Default.PickAsync(PickOptions);
    }

    private const string PickerTitleText = "Choose a database backup";
}
