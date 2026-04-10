#if ANDROID
using Android.Content;
using Android.Provider;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidDatabaseExportFileSaver : IDatabaseExportFileSaver
{
    private const string DatabaseMimeType = "application/octet-stream";

    public async Task<DatabaseExportResult> SaveBackupAsync(
        string backupFilePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(backupFilePath);

        if (!File.Exists(backupFilePath))
            throw new FileNotFoundException("The generated backup file could not be found.", backupFilePath);

        var contentResolver = Application.Context.ContentResolver
                              ?? throw new InvalidOperationException("An Android content resolver is required to export database backups.");

        var targetUri = InsertDownloadRecord(contentResolver, Path.GetFileName(backupFilePath));

        try
        {
            await using var sourceStream = File.OpenRead(backupFilePath);
            await using var destinationStream = contentResolver.OpenOutputStream(targetUri)
                                                ?? throw new InvalidOperationException("The Android Downloads destination could not be opened.");

            await sourceStream.CopyToAsync(destinationStream, cancellationToken);

            using var completionValues = new ContentValues();
            completionValues.Put(MediaStore.IMediaColumns.IsPending, 0);
            contentResolver.Update(targetUri, completionValues, null, null);

            return DatabaseExportResult.Saved;
        }
        catch
        {
            contentResolver.Delete(targetUri, null, null);
            throw;
        }
    }

    private static Uri InsertDownloadRecord(ContentResolver contentResolver, string fileName)
    {
        using var values = new ContentValues();
        values.Put(MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(MediaStore.IMediaColumns.MimeType, DatabaseMimeType);
        values.Put(MediaStore.IMediaColumns.RelativePath, Environment.DirectoryDownloads);
        values.Put(MediaStore.IMediaColumns.IsPending, 1);

        return contentResolver.Insert(MediaStore.Downloads.GetContentUri(MediaStore.VolumeExternalPrimary), values)
               ?? throw new InvalidOperationException("The Android Downloads destination could not be created.");
    }
}
#endif