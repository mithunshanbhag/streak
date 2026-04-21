#if ANDROID
namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDatabaseImportFilePicker : IDatabaseImportFilePicker
{
    private const string PickerTitleText = "Choose a data backup or database";

    private static readonly PickOptions PickOptions = new()
    {
        PickerTitle = PickerTitleText,
        FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            [DevicePlatform.Android] =
            [
                "application/zip",
                "application/x-zip-compressed",
                "application/x-sqlite3",
                "application/vnd.sqlite3",
                "application/octet-stream",
                "*/*"
            ]
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
