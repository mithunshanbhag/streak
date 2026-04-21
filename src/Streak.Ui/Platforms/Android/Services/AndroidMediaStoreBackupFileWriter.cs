#if ANDROID
using Android.Content;
using Android.Provider;
using Application = Android.App.Application;
using Uri = Android.Net.Uri;

namespace Streak.Ui.Services.Implementations;

internal static class AndroidMediaStoreBackupFileWriter
{
    private const string BackupMimeType = "application/zip";

    public static Task<SavedFileLocation> SaveBackupAsync(
        string sourceFilePath,
        string relativePath,
        string? displayRelativePath = null,
        CancellationToken cancellationToken = default)
    {
        return SaveFileAsync(
            sourceFilePath,
            relativePath,
            BackupMimeType,
            displayRelativePath,
            cancellationToken);
    }

    public static async Task<SavedFileLocation> SaveFileAsync(
        string sourceFilePath,
        string relativePath,
        string mimeType,
        string? displayRelativePath = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("The generated backup file could not be found.", sourceFilePath);

        var normalizedRelativePath = relativePath.TrimEnd('/', '\\');
        var normalizedDisplayRelativePath = (displayRelativePath ?? normalizedRelativePath).TrimEnd('/', '\\');
        var fileName = Path.GetFileName(sourceFilePath);
        var contentResolver = Application.Context.ContentResolver
                              ?? throw new InvalidOperationException("An Android content resolver is required to persist data backups.");
        var targetUri = InsertDownloadRecord(contentResolver, fileName, normalizedRelativePath, mimeType);

        try
        {
            await using (var sourceStream = File.OpenRead(sourceFilePath))
            await using (var destinationStream = contentResolver.OpenOutputStream(targetUri)
                                              ?? throw new InvalidOperationException("The Android backup destination could not be opened."))
            {
                await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            }

            FinalizePendingDownloadRecord(contentResolver, targetUri);

            return new SavedFileLocation
            {
                SavedFileDisplayPath = $"{normalizedDisplayRelativePath}/{fileName}",
                ParentFolderDisplayPath = normalizedDisplayRelativePath
            };
        }
        catch
        {
            contentResolver.Delete(targetUri, null, null);
            throw;
        }
    }

    private static Uri InsertDownloadRecord(
        ContentResolver contentResolver,
        string fileName,
        string relativePath,
        string mimeType)
    {
        using var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(MediaStore.IMediaColumns.MimeType, mimeType);
        values.Put(MediaStore.IMediaColumns.RelativePath, relativePath);
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        return contentResolver.Insert(MediaStore.Downloads.GetContentUri(MediaStore.VolumeExternalPrimary), values)
               ?? throw new InvalidOperationException("The Android backup destination could not be created.");
    }

    private static void FinalizePendingDownloadRecord(ContentResolver contentResolver, Uri targetUri)
    {
        using var completionValues = new ContentValues();
        completionValues.Put(MediaStore.IMediaColumns.IsPending, 0);

        var updatedRowCount = contentResolver.Update(targetUri, completionValues, null, null);
        if (updatedRowCount <= 0)
            throw new InvalidOperationException("The Android export destination could not be finalized.");
    }
}
#endif
