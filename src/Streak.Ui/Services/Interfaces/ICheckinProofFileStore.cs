namespace Streak.Ui.Services.Interfaces;

public interface ICheckinProofFileStore
{
    Task SaveAsync(
        string proofImageUri,
        Stream sourceStream,
        string mimeType,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string proofImageUri, CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(string proofImageUri, CancellationToken cancellationToken = default);

    Task DeleteIfExistsAsync(string? proofImageUri, CancellationToken cancellationToken = default);

    Task DeleteAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAllProofImageUrisAsync(CancellationToken cancellationToken = default);
}
