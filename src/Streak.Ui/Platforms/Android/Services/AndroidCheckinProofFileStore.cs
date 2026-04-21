#if ANDROID
using Android.Content;
using Android.Provider;
using Application = Android.App.Application;
using Environment = Android.OS.Environment;
using Uri = Android.Net.Uri;

namespace Streak.Ui.Services.Implementations;

public sealed class AndroidCheckinProofFileStore : ICheckinProofFileStore
{
    private static readonly Uri ExternalImagesUri = MediaStore.Images.Media.GetContentUri(MediaStore.VolumeExternalPrimary);

    private static readonly string RootRelativePath = string.Join(
        '/',
        Environment.DirectoryPictures ?? throw new InvalidOperationException("Android shared pictures storage is unavailable."),
        StreakExportStorageConstants.AndroidRootDirectoryName,
        CheckinProofStorageConstants.CheckinProofsDirectoryName);

    public async Task SaveAsync(
        string proofImageUri,
        Stream sourceStream,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        var targetUri = FindProofUri(normalizedRelativeProofPath);
        if (targetUri is not null)
            DeleteByUri(GetContentResolver(), targetUri);

        var fileName = CheckinProofPathUtility.GetFileName(normalizedRelativeProofPath);
        var relativeDirectoryPath = GetMediaStoreDirectoryPath(normalizedRelativeProofPath);
        var contentResolver = GetContentResolver();
        targetUri = InsertImageRecord(contentResolver, fileName, relativeDirectoryPath, mimeType);

        try
        {
            await using var destinationStream = contentResolver.OpenOutputStream(targetUri)
                                              ?? throw new InvalidOperationException("The Android proof destination could not be opened.");
            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            FinalizePendingRecord(contentResolver, targetUri);
        }
        catch
        {
            DeleteByUri(contentResolver, targetUri);
            throw;
        }
    }

    public Task<bool> ExistsAsync(string proofImageUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        return Task.FromResult(FindProofUri(normalizedRelativeProofPath) is not null);
    }

    public Task<Stream> OpenReadAsync(string proofImageUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        var proofUri = FindProofUri(normalizedRelativeProofPath)
                       ?? throw new FileNotFoundException("The requested Android check-in proof file could not be found.", normalizedRelativeProofPath);

        return Task.FromResult<Stream>(
            GetContentResolver().OpenInputStream(proofUri)
            ?? throw new InvalidOperationException("The Android proof source could not be opened."));
    }

    public Task DeleteIfExistsAsync(string? proofImageUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(proofImageUri))
            return Task.CompletedTask;

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        var proofUri = FindProofUri(normalizedRelativeProofPath);
        if (proofUri is not null)
            DeleteByUri(GetContentResolver(), proofUri);

        return Task.CompletedTask;
    }

    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var contentResolver = GetContentResolver();
        foreach (var proofImageUri in GetAllProofImageUrisInternal(contentResolver))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var proofUri = FindProofUri(proofImageUri);
            if (proofUri is not null)
                DeleteByUri(contentResolver, proofUri);
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetAllProofImageUrisAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetAllProofImageUrisInternal(GetContentResolver()));
    }

    private static ContentResolver GetContentResolver()
    {
        return Application.Context.ContentResolver
               ?? throw new InvalidOperationException("An Android content resolver is required to access shared proof storage.");
    }

    private static Uri? FindProofUri(string normalizedRelativeProofPath)
    {
        var contentResolver = GetContentResolver();
        var fileName = CheckinProofPathUtility.GetFileName(normalizedRelativeProofPath);
        var relativeDirectoryPath = GetMediaStoreDirectoryPath(normalizedRelativeProofPath);
        var selection =
            $"({MediaStore.IMediaColumns.RelativePath} = ? OR {MediaStore.IMediaColumns.RelativePath} = ?) AND {MediaStore.IMediaColumns.DisplayName} = ?";

        using var cursor = contentResolver.Query(
            ExternalImagesUri,
            [BaseColumns.Id],
            selection,
            [relativeDirectoryPath, $"{relativeDirectoryPath}/", fileName],
            null);

        if (cursor is null || !cursor.MoveToFirst())
            return null;

        var idColumnIndex = cursor.GetColumnIndexOrThrow(BaseColumns.Id);
        var mediaId = cursor.GetLong(idColumnIndex);
        return ContentUris.WithAppendedId(ExternalImagesUri, mediaId);
    }

    private static IReadOnlyList<string> GetAllProofImageUrisInternal(ContentResolver contentResolver)
    {
        var selection =
            $"{MediaStore.IMediaColumns.RelativePath} = ? OR {MediaStore.IMediaColumns.RelativePath} LIKE ?";

        using var cursor = contentResolver.Query(
            ExternalImagesUri,
            [MediaStore.IMediaColumns.RelativePath, MediaStore.IMediaColumns.DisplayName],
            selection,
            [$"{RootRelativePath}/", $"{RootRelativePath}/%"],
            null);

        if (cursor is null)
            return [];

        var relativePathColumnIndex = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.RelativePath);
        var displayNameColumnIndex = cursor.GetColumnIndexOrThrow(MediaStore.IMediaColumns.DisplayName);
        var proofImageUris = new List<string>();

        while (cursor.MoveToNext())
        {
            var relativePath = NormalizeMediaStoreRelativePath(cursor.GetString(relativePathColumnIndex));
            var displayName = cursor.GetString(displayNameColumnIndex);
            if (string.IsNullOrWhiteSpace(displayName))
                continue;

            if (!TryBuildRelativeProofPath(relativePath, displayName, out var relativeProofPath))
                continue;

            proofImageUris.Add(relativeProofPath);
        }

        proofImageUris.Sort(StringComparer.Ordinal);
        return proofImageUris;
    }

    private static bool TryBuildRelativeProofPath(
        string normalizedMediaStoreRelativePath,
        string displayName,
        out string relativeProofPath)
    {
        relativeProofPath = string.Empty;

        if (string.IsNullOrWhiteSpace(displayName))
            return false;

        var rootWithSeparator = $"{RootRelativePath}/";
        if (!normalizedMediaStoreRelativePath.StartsWith(rootWithSeparator, StringComparison.Ordinal))
            return false;

        var nestedRelativePath = normalizedMediaStoreRelativePath[rootWithSeparator.Length..].Trim('/');
        relativeProofPath = string.IsNullOrWhiteSpace(nestedRelativePath)
            ? displayName
            : $"{nestedRelativePath}/{displayName}";

        return CheckinProofPathUtility.TryNormalizeRelativeProofPath(relativeProofPath, out relativeProofPath);
    }

    private static string GetMediaStoreDirectoryPath(string normalizedRelativeProofPath)
    {
        var directoryRelativePath = CheckinProofPathUtility.GetDirectoryRelativePath(normalizedRelativeProofPath);
        return string.IsNullOrWhiteSpace(directoryRelativePath)
            ? RootRelativePath
            : $"{RootRelativePath}/{directoryRelativePath}";
    }

    private static string NormalizeMediaStoreRelativePath(string? relativePath)
    {
        return (relativePath ?? string.Empty)
            .Replace('\\', '/')
            .Trim()
            .Trim('/');
    }

    private static Uri InsertImageRecord(
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

        return contentResolver.Insert(ExternalImagesUri, values)
               ?? throw new InvalidOperationException("The Android proof destination could not be created.");
    }

    private static void FinalizePendingRecord(ContentResolver contentResolver, Uri targetUri)
    {
        using var completionValues = new ContentValues();
        completionValues.Put(MediaStore.IMediaColumns.IsPending, 0);

        var updatedRowCount = contentResolver.Update(targetUri, completionValues, null, null);
        if (updatedRowCount <= 0)
            throw new InvalidOperationException("The Android proof destination could not be finalized.");
    }

    private static void DeleteByUri(ContentResolver contentResolver, Uri proofUri)
    {
        contentResolver.Delete(proofUri, null, null);
    }
}
#endif
