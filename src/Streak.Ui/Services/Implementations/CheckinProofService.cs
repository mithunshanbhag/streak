namespace Streak.Ui.Services.Implementations;

public sealed class CheckinProofService(
    ICheckinProofMediaPickerService mediaPickerService,
    ICameraPermissionService cameraPermissionService,
    ICheckinProofFileStore checkinProofFileStore) : ICheckinProofService
{
    private readonly ICheckinProofMediaPickerService _mediaPickerService = mediaPickerService;
    private readonly ICameraPermissionService _cameraPermissionService = cameraPermissionService;
    private readonly ICheckinProofFileStore _checkinProofFileStore = checkinProofFileStore;

    public bool SupportsCameraCapture => _mediaPickerService.SupportsCameraCapture;

    public async Task<CheckinProofSelection?> CapturePhotoAsync(CancellationToken cancellationToken = default)
    {
        if (!SupportsCameraCapture)
            return null;

        var cameraPermissionGranted = await _cameraPermissionService.RequestPermissionIfNeededAsync(cancellationToken);
        if (!cameraPermissionGranted)
            throw new InvalidOperationException(CameraPermissionRequiredErrorText);

        var fileResult = await _mediaPickerService.CapturePhotoAsync(cancellationToken);
        return await CreateSelectionAsync(fileResult, CheckinProofSource.Camera, cancellationToken);
    }

    public async Task<CheckinProofSelection?> PickPhotoAsync(CancellationToken cancellationToken = default)
    {
        var fileResult = await _mediaPickerService.PickPhotoAsync(cancellationToken);
        return await CreateSelectionAsync(fileResult, CheckinProofSource.Gallery, cancellationToken);
    }

    public async Task<CheckinProofInputModel> PersistAsync(
        CheckinProofSelection selection,
        int habitId,
        string checkinDate,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selection);

        if (habitId <= 0)
            throw new ArgumentOutOfRangeException(nameof(habitId), "Habit ID must be greater than zero.");

        if (!DateOnly.TryParseExact(
                checkinDate,
                CoreConstants.CheckinDateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out _))
            throw new ArgumentException(
                $"Date must match format '{CoreConstants.CheckinDateFormat}'.",
                nameof(checkinDate));

        if (selection.FileSizeBytes is <= 0 or > CoreConstants.CheckinProofMaxSizeBytes)
            throw new InvalidOperationException(
                $"Selected picture proof must be {FormatFileSize(CoreConstants.CheckinProofMaxSizeBytes)} or smaller.");

        var relativePath = BuildRelativeProofPath(habitId, checkinDate, selection.FileExtension);
        await using var sourceStream = new MemoryStream(selection.FileBytes, writable: false);
        await _checkinProofFileStore.SaveAsync(
            relativePath,
            sourceStream,
            CheckinProofFileUtility.GetMimeType(selection.FileExtension),
            cancellationToken);

        return new CheckinProofInputModel
        {
            ProofImageUri = relativePath,
            ProofImageDisplayName = selection.DisplayName,
            ProofImageSizeBytes = selection.FileSizeBytes,
            ProofImageModifiedOn = selection.ModifiedOn
        };
    }

    public Task DeleteIfExistsAsync(string? proofImageUri, CancellationToken cancellationToken = default)
    {
        return _checkinProofFileStore.DeleteIfExistsAsync(proofImageUri, cancellationToken);
    }

    private async Task<CheckinProofSelection?> CreateSelectionAsync(
        FileResult? fileResult,
        CheckinProofSource source,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (fileResult is null)
            return null;

        await using var fileStream = await fileResult.OpenReadAsync();
        var fileBytes = await ReadFileBytesAsync(fileStream, cancellationToken);
        var fileExtension = NormalizeFileExtension(fileResult.FileName, fileResult.FullPath);
        var mimeType = CheckinProofFileUtility.GetMimeType(fileExtension);
        var modifiedOn = GetModifiedOn(fileResult.FullPath);

        return new CheckinProofSelection
        {
            DisplayName = ResolveDisplayName(fileResult.FileName, fileExtension),
            FileExtension = fileExtension,
            FileBytes = fileBytes,
            ModifiedOn = modifiedOn.ToString(CoreConstants.CheckinProofModifiedOnFormat, CultureInfo.InvariantCulture),
            PreviewDataUrl = $"data:{mimeType};base64,{Convert.ToBase64String(fileBytes)}",
            Source = source,
            SourceDescription = source == CheckinProofSource.Camera
                ? "Camera"
                : "Gallery"
        };
    }

    private static async Task<byte[]> ReadFileBytesAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var memoryStream = new MemoryStream();
        var buffer = new byte[81920];

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
            if (bytesRead == 0)
                break;

            await memoryStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);

            if (memoryStream.Length > CoreConstants.CheckinProofMaxSizeBytes)
                throw new InvalidOperationException(
                    $"Selected picture proof must be {FormatFileSize(CoreConstants.CheckinProofMaxSizeBytes)} or smaller.");
        }

        if (memoryStream.Length == 0)
            throw new InvalidOperationException("Selected picture proof is empty.");

        return memoryStream.ToArray();
    }

    private string BuildRelativeProofPath(int habitId, string checkinDate, string fileExtension)
    {
        var date = DateOnly.ParseExact(checkinDate, CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture);
        var normalizedExtension = CheckinProofFileUtility.NormalizeStoredFileExtension(fileExtension);

        return string.Join(
            '/',
            $"Habit-{habitId}",
            date.Year.ToString("0000", CultureInfo.InvariantCulture),
            date.Month.ToString("00", CultureInfo.InvariantCulture),
            date.ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture),
            $"{Guid.NewGuid():N}{normalizedExtension}");
    }

    private static string ResolveDisplayName(string? fileName, string fileExtension)
    {
        var normalizedFileName = string.IsNullOrWhiteSpace(fileName)
            ? null
            : Path.GetFileName(fileName.Trim());

        if (!string.IsNullOrWhiteSpace(normalizedFileName))
            return normalizedFileName;

        return $"proof{CheckinProofFileUtility.NormalizeStoredFileExtension(fileExtension)}";
    }

    private static string NormalizeFileExtension(string? fileName, string? fullPath)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            extension = Path.GetExtension(fullPath);

        return CheckinProofFileUtility.NormalizeStoredFileExtension(extension);
    }

    private static DateTimeOffset GetModifiedOn(string? fullPath)
    {
        if (!string.IsNullOrWhiteSpace(fullPath) && File.Exists(fullPath))
            return File.GetLastWriteTimeUtc(fullPath);

        return DateTimeOffset.UtcNow;
    }

    private static string FormatFileSize(long fileSizeBytes)
    {
        return $"{fileSizeBytes / (1024d * 1024d):0.#} MB";
    }

    private const string CameraPermissionRequiredErrorText =
        "Camera permission is required to take a picture. You can continue without a photo or choose Gallery instead.";
}
