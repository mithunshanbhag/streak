namespace Streak.Ui.Services.Implementations;

public sealed class FileSystemCheckinProofFileStore(IAppStoragePathService appStoragePathService) : ICheckinProofFileStore
{
    private readonly IAppStoragePathService _appStoragePathService = appStoragePathService;

    public async Task SaveAsync(
        string proofImageUri,
        Stream sourceStream,
        string mimeType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);
        ArgumentNullException.ThrowIfNull(sourceStream);

        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        var absolutePath = GetAbsolutePath(normalizedRelativeProofPath);
        var directoryPath = Path.GetDirectoryName(absolutePath)
                            ?? throw new InvalidOperationException("Unable to determine the proof storage directory.");

        Directory.CreateDirectory(directoryPath);
        await using var destinationStream = File.Create(absolutePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    public Task<bool> ExistsAsync(string proofImageUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        return Task.FromResult(File.Exists(GetAbsolutePath(normalizedRelativeProofPath)));
    }

    public Task<Stream> OpenReadAsync(string proofImageUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        var absolutePath = GetAbsolutePath(normalizedRelativeProofPath);

        if (!File.Exists(absolutePath))
            throw new FileNotFoundException("The requested check-in proof file could not be found.", absolutePath);

        return Task.FromResult<Stream>(File.OpenRead(absolutePath));
    }

    public Task DeleteIfExistsAsync(string? proofImageUri, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(proofImageUri))
            return Task.CompletedTask;

        var normalizedRelativeProofPath = CheckinProofPathUtility.NormalizeRelativeProofPath(proofImageUri);
        var absolutePath = GetAbsolutePath(normalizedRelativeProofPath);
        if (File.Exists(absolutePath))
            File.Delete(absolutePath);

        return Task.CompletedTask;
    }

    public Task DeleteAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (Directory.Exists(_appStoragePathService.CheckinProofsDirectoryPath))
            Directory.Delete(_appStoragePathService.CheckinProofsDirectoryPath, true);

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> GetAllProofImageUrisAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(_appStoragePathService.CheckinProofsDirectoryPath))
            return Task.FromResult<IReadOnlyList<string>>([]);

        var proofImageUris = Directory
            .GetFiles(_appStoragePathService.CheckinProofsDirectoryPath, "*", SearchOption.AllDirectories)
            .Select(sourceFilePath => Path.GetRelativePath(_appStoragePathService.CheckinProofsDirectoryPath, sourceFilePath)
                .Replace(Path.DirectorySeparatorChar, '/'))
            .OrderBy(relativeProofPath => relativeProofPath, StringComparer.Ordinal)
            .ToList();

        return Task.FromResult<IReadOnlyList<string>>(proofImageUris);
    }

    private string GetAbsolutePath(string normalizedRelativeProofPath)
    {
        return CheckinProofPathUtility.GetAbsolutePath(
            _appStoragePathService.CheckinProofsDirectoryPath,
            normalizedRelativeProofPath);
    }
}
