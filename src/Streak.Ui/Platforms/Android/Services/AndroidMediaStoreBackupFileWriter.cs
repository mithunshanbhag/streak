#if ANDROID
using Android.Content;
using Android.Provider;
using Application = Android.App.Application;
using Uri = Android.Net.Uri;

namespace Streak.Ui.Services.Implementations;

internal static class AndroidMediaStoreBackupFileWriter
{
    private const string DatabaseMimeType = "application/octet-stream";

    public static Task<string> SaveBackupAsync(
        string sourceFilePath,
        string relativePath,
        CancellationToken cancellationToken = default)
    {
        return SaveFileAsync(
            sourceFilePath,
            relativePath,
            DatabaseMimeType,
            cancellationToken);
    }

    public static async Task<string> SaveFileAsync(
        string sourceFilePath,
        string relativePath,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceFilePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("The generated backup file could not be found.", sourceFilePath);

        var normalizedRelativePath = relativePath.TrimEnd('/', '\\');
        var fileName = Path.GetFileName(sourceFilePath);
        var contentResolver = Application.Context.ContentResolver
                              ?? throw new InvalidOperationException("An Android content resolver is required to persist database backups.");
        var targetUri = InsertDownloadRecord(contentResolver, fileName, normalizedRelativePath, mimeType);

        try
        {
            await using var sourceStream = File.OpenRead(sourceFilePath);
            await using var destinationStream = contentResolver.OpenOutputStream(targetUri)
                                                ?? throw new InvalidOperationException("The Android backup destination could not be opened.");

            await sourceStream.CopyToAsync(destinationStream, cancellationToken);

            using var completionValues = new ContentValues();
            completionValues.Put(MediaStore.IMediaColumns.IsPending, 0);
            contentResolver.Update(targetUri, completionValues, null, null);

            return $"{normalizedRelativePath}/{fileName}";
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
}
#endif
