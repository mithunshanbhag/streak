using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveBackupUploadClient(
    HttpClient httpClient,
    IOneDriveAuthService oneDriveAuthService,
    ILogger<OneDriveBackupUploadClient> logger)
    : IOneDriveBackupUploadClient
{
    private const string GraphApiRoot = "me/drive/special/approot";

    private readonly HttpClient _httpClient = httpClient;
    private readonly IOneDriveAuthService _oneDriveAuthService = oneDriveAuthService;
    private readonly ILogger<OneDriveBackupUploadClient> _logger = logger;

    public async Task UploadManualBackupAsync(
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("The local backup archive could not be found.", localFilePath);

        try
        {
            var accessToken = await _oneDriveAuthService.GetAccessTokenAsync(cancellationToken);

            await EnsureFolderExistsAsync(accessToken, null, StreakExportStorageConstants.BackupsDirectoryName, cancellationToken);
            await EnsureFolderExistsAsync(
                accessToken,
                StreakExportStorageConstants.BackupsDirectoryName,
                StreakExportStorageConstants.ManualBackupsDirectoryName,
                cancellationToken);

            await UploadFileAsync(accessToken, localFilePath, destinationFileName, cancellationToken);
        }
        catch (OneDriveBackupException)
        {
            throw;
        }
        catch (OneDriveAuthenticationRequiredException exception)
        {
            throw new OneDriveBackupException(
                OneDriveBackupFailureKind.AuthRequired,
                "OneDrive needs you to reconnect before backing up again.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            throw new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now.",
                exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "The OneDrive upload timed out before it completed.",
                exception);
        }
    }

    private async Task EnsureFolderExistsAsync(
        string accessToken,
        string? parentPath,
        string folderName,
        CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            BuildChildrenEndpoint(parentPath),
            accessToken);
        request.Content = JsonContent.Create(new CreateFolderRequest
        {
            Name = folderName
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict)
            return;

        await ThrowForGraphFailureAsync(response, cancellationToken);
    }

    private async Task UploadFileAsync(
        string accessToken,
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(localFilePath);

        using var request = CreateAuthorizedRequest(
            HttpMethod.Put,
            BuildUploadEndpoint(destinationFileName),
            accessToken);
        request.Content = new StreamContent(fileStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
            return;

        await ThrowForGraphFailureAsync(response, cancellationToken);
    }

    private async Task ThrowForGraphFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var graphError = await ReadGraphErrorAsync(response, cancellationToken);
        var failureKind = ClassifyFailure(response.StatusCode, graphError?.Code);

        _logger.LogWarning(
            "OneDrive Graph request failed. Status code: {StatusCode}. Graph error code: {GraphErrorCode}. Failure kind: {FailureKind}.",
            (int)response.StatusCode,
            graphError?.Code,
            failureKind);

        throw new OneDriveBackupException(
            failureKind,
            BuildFailureMessage(failureKind, graphError?.Message));
    }

    private static HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string requestUri, string accessToken)
    {
        var request = new HttpRequestMessage(method, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    private static async Task<GraphErrorResponse?> ReadGraphErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            var envelope = await response.Content.ReadFromJsonAsync<GraphErrorEnvelope>(cancellationToken);
            return envelope?.Error;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static OneDriveBackupFailureKind ClassifyFailure(HttpStatusCode statusCode, string? graphErrorCode)
    {
        if (statusCode == HttpStatusCode.Unauthorized || statusCode == HttpStatusCode.Forbidden)
            return OneDriveBackupFailureKind.AuthRequired;

        if (statusCode == HttpStatusCode.InsufficientStorage
            || statusCode == HttpStatusCode.RequestEntityTooLarge
            || ContainsQuotaCode(graphErrorCode))
        {
            return OneDriveBackupFailureKind.QuotaExceeded;
        }

        if (statusCode == HttpStatusCode.RequestTimeout
            || statusCode == (HttpStatusCode)429
            || (int)statusCode >= 500)
        {
            return OneDriveBackupFailureKind.NetworkUnavailable;
        }

        return OneDriveBackupFailureKind.Unknown;
    }

    private static bool ContainsQuotaCode(string? graphErrorCode)
    {
        if (string.IsNullOrWhiteSpace(graphErrorCode))
            return false;

        return graphErrorCode.Contains("quota", StringComparison.OrdinalIgnoreCase)
               || graphErrorCode.Contains("insufficient", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildFailureMessage(OneDriveBackupFailureKind failureKind, string? graphMessage)
    {
        return failureKind switch
        {
            OneDriveBackupFailureKind.AuthRequired => "OneDrive needs you to reconnect before backing up again.",
            OneDriveBackupFailureKind.NetworkUnavailable => "Unable to reach OneDrive right now. Check your connection and try again.",
            OneDriveBackupFailureKind.QuotaExceeded => "Your OneDrive storage is full. Free up space in OneDrive and try again.",
            _ => string.IsNullOrWhiteSpace(graphMessage)
                ? "The OneDrive backup failed."
                : $"The OneDrive backup failed: {graphMessage}"
        };
    }

    private static string BuildChildrenEndpoint(string? parentPath)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
            return $"{GraphApiRoot}/children";

        return $"{GraphApiRoot}:/{EncodePath(parentPath)}:/children";
    }

    private static string BuildUploadEndpoint(string destinationFileName)
    {
        var remotePath = string.Join(
            '/',
            [
                StreakExportStorageConstants.BackupsDirectoryName,
                StreakExportStorageConstants.ManualBackupsDirectoryName,
                destinationFileName
            ]);

        return $"{GraphApiRoot}:/{EncodePath(remotePath)}:/content";
    }

    private static string EncodePath(string path)
    {
        return string.Join(
            '/',
            path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(Uri.EscapeDataString));
    }

    private sealed class CreateFolderRequest
    {
        public required string Name { get; init; }

        public EmptyFolderPayload Folder { get; init; } = new();

        [JsonPropertyName("@microsoft.graph.conflictBehavior")]
        public string ConflictBehavior { get; init; } = "fail";
    }

    private sealed class EmptyFolderPayload;

    private sealed class GraphErrorEnvelope
    {
        public GraphErrorResponse? Error { get; init; }
    }

    private sealed class GraphErrorResponse
    {
        public string? Code { get; init; }

        public string? Message { get; init; }
    }
}
