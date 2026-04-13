#if ANDROID
namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDatabaseImportFilePicker : IDatabaseImportFilePicker
{
    private const string PickerTitleText = "Choose a database backup";

    private static readonly PickOptions PickOptions = new()
    {
        PickerTitle = PickerTitleText,
        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.Android] = ["application/octet-stream", "application/x-sqlite3", "application/vnd.sqlite3"]
        })
    };

    public async Task<FileResult?> PickBackupAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fileResult = await FilePicker.Default.PickAsync(PickOptions);
        cancellationToken.ThrowIfCancellationRequested();

        return fileResult;
    }
}
#endif