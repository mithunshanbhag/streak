using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Streak.Ui.Services.Implementations;

public sealed class OneDriveBackupUploadClient(
    HttpClient httpClient,
    IOneDriveAuthService oneDriveAuthService,
    IConnectivity connectivity,
    ILogger<OneDriveBackupUploadClient> logger)
    : IOneDriveBackupUploadClient
{
    private const string GraphAppRoot = "me/drive/special/approot";

    private readonly HttpClient _httpClient = httpClient;
    private readonly IOneDriveAuthService _oneDriveAuthService = oneDriveAuthService;
    private readonly IConnectivity _connectivity = connectivity;
    private readonly ILogger<OneDriveBackupUploadClient> _logger = logger;

    public async Task UploadManualBackupAsync(
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken = default)
    {
        await UploadBackupAsync(
            localFilePath,
            destinationFileName,
            StreakExportStorageConstants.ManualBackupsDirectoryName,
            "Manual",
            "EnsureOneDriveManualBackupsFolder",
            "UploadManualBackupArchive",
            cancellationToken);
    }

    public async Task UploadAutomatedBackupAsync(
        string localFilePath,
        string destinationFileName,
        CancellationToken cancellationToken = default)
    {
        await UploadBackupAsync(
            localFilePath,
            destinationFileName,
            StreakExportStorageConstants.AutomatedBackupsDirectoryName,
            "Automated",
            "EnsureOneDriveAutomatedBackupsFolder",
            "UploadAutomatedBackupArchive",
            cancellationToken);
    }

    private async Task UploadBackupAsync(
        string localFilePath,
        string destinationFileName,
        string targetDirectoryName,
        string backupKindLabel,
        string ensureFolderOperationName,
        string uploadOperationName,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("The local backup archive could not be found.", localFilePath);

        var fileInfo = new FileInfo(localFilePath);
        using var scope = _logger.BeginScope(new Dictionary<string, object> { ["FileName"] = destinationFileName });
        _logger.LogInformation(
            "{BackupKind} OneDrive backup upload starting. File name: {FileName}. File size bytes: {FileSizeBytes}. Target folder: {TargetFolder}. Network access: {NetworkAccess}. Connection profiles: {ConnectionProfiles}.",
            backupKindLabel,
            destinationFileName,
            fileInfo.Length,
            OneDriveAuthConstants.StorageLocationDisplayName,
            _connectivity.NetworkAccess,
            GetConnectionProfilesDisplay());

        var uploadStage = "AccessTokenAcquisition";
        try
        {
            _logger.LogInformation(
                "{BackupKind} OneDrive access token acquisition starting. File name: {FileName}.",
                backupKindLabel,
                destinationFileName);
            var accessToken = await _oneDriveAuthService.GetAccessTokenAsync(cancellationToken);
            _logger.LogInformation(
                "{BackupKind} OneDrive access token acquisition completed. File name: {FileName}.",
                backupKindLabel,
                destinationFileName);

            uploadStage = "EnsureAppFolderAccessible";
            var rootId = await EnsureAppFolderAccessibleAsync(accessToken, cancellationToken);
            uploadStage = "EnsureOneDriveBackupsFolder";
            var backupsId = await EnsureFolderExistsAsync(
                accessToken,
                parentId: rootId,
                folderName: StreakExportStorageConstants.BackupsDirectoryName,
                operationName: "EnsureOneDriveBackupsFolder",
                cancellationToken);
            uploadStage = ensureFolderOperationName;
            var destinationId = await EnsureFolderExistsAsync(
                accessToken,
                parentId: backupsId,
                folderName: targetDirectoryName,
                operationName: ensureFolderOperationName,
                cancellationToken);
            uploadStage = uploadOperationName;
            await UploadFileAsync(
                accessToken,
                localFilePath,
                destinationFileName,
                destinationId,
                uploadOperationName,
                cancellationToken);

            _logger.LogInformation(
                "{BackupKind} OneDrive backup upload completed. File name: {FileName}. File size bytes: {FileSizeBytes}. Target folder: {TargetFolder}.",
                backupKindLabel,
                destinationFileName,
                fileInfo.Length,
                OneDriveAuthConstants.StorageLocationDisplayName);
        }
        catch (OneDriveBackupException exception)
        {
            _logger.LogWarning("{BackupKind} OneDrive backup failed during {UploadStage}. File name: {FileName}. Failure kind: {FailureKind}. Message: {FailureMessage}.",
                backupKindLabel, uploadStage, destinationFileName, exception.FailureKind, exception.Message);
            throw;
        }
        catch (OneDriveAuthenticationRequiredException exception)
        {
            _logger.LogInformation(
                exception,
                "{BackupKind} OneDrive backup requires reconnect during {UploadStage}. File name: {FileName}.",
                backupKindLabel,
                uploadStage,
                destinationFileName);
            throw new OneDriveBackupException(
                OneDriveBackupFailureKind.AuthRequired,
                "OneDrive needs you to reconnect before backing up again.",
                exception);
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "{BackupKind} OneDrive backup network failure during {UploadStage}. File name: {FileName}. Network access: {NetworkAccess}. Connection profiles: {ConnectionProfiles}. Failure message: {FailureMessage}.",
                backupKindLabel,
                uploadStage,
                destinationFileName,
                _connectivity.NetworkAccess,
                GetConnectionProfilesDisplay(),
                exception.Message);
            throw new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "Unable to reach OneDrive right now.",
                exception);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                exception,
                "{BackupKind} OneDrive backup timed out during {UploadStage}. File name: {FileName}. Network access: {NetworkAccess}. Connection profiles: {ConnectionProfiles}.",
                backupKindLabel,
                uploadStage,
                destinationFileName,
                _connectivity.NetworkAccess,
                GetConnectionProfilesDisplay());
            throw new OneDriveBackupException(
                OneDriveBackupFailureKind.NetworkUnavailable,
                "The OneDrive upload timed out before it completed.",
                exception);
        }
    }

    private async Task<string> EnsureAppFolderAccessibleAsync(
        string accessToken,
        CancellationToken cancellationToken)
    {
        const string operationName = "GetOneDriveAppFolder";
        using var request = CreateAuthorizedRequest(HttpMethod.Get, GraphAppRoot, accessToken);

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "OneDrive Graph request starting. Operation: {GraphOperation}. Method: {HttpMethod}. Relative URI: {RelativeUri}.",
            operationName,
            request.Method.Method,
            request.RequestUri?.ToString());

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        LogGraphResponse(operationName, response, stopwatch.ElapsedMilliseconds);

        if (response.IsSuccessStatusCode)
            return await ReadFolderIdAsync(response, cancellationToken);

        await ThrowForGraphFailureAsync(response, operationName, cancellationToken);
        throw new InvalidOperationException("Graph failure handler returned unexpectedly.");
    }

    private async Task<string> EnsureFolderExistsAsync(
        string accessToken,
        string parentId,
        string folderName,
        string operationName,
        CancellationToken cancellationToken)
    {
        var lookupUri = $"me/drive/items/{Uri.EscapeDataString(parentId)}:/{Uri.EscapeDataString(folderName)}";
        var existingId = await LookupFolderAsync(accessToken, lookupUri, operationName + "Lookup", true, cancellationToken);
        if (existingId is not null)
            return existingId;

        using var request = CreateAuthorizedRequest(
            HttpMethod.Post,
            $"me/drive/items/{Uri.EscapeDataString(parentId)}/children",
            accessToken);
        request.Content = JsonContent.Create(new CreateFolderRequest
        {
            Name = folderName
        });

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "OneDrive Graph request starting. Operation: {GraphOperation}. Method: {HttpMethod}. Relative URI: {RelativeUri}. Folder name: {FolderName}.",
            operationName,
            request.Method.Method,
            request.RequestUri?.ToString(),
            folderName);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        LogGraphResponse(operationName, response, stopwatch.ElapsedMilliseconds);

        if (response.IsSuccessStatusCode)
            return await ReadFolderIdAsync(response, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Conflict)
            return (await LookupFolderAsync(accessToken, lookupUri, operationName + "ConflictLookup", false, cancellationToken))!;

        await ThrowForGraphFailureAsync(response, operationName, cancellationToken);
        throw new InvalidOperationException("Graph failure handler returned unexpectedly.");
    }

    private async Task<string?> LookupFolderAsync(string accessToken, string uri, string operationName, bool allowMissing, CancellationToken cancellationToken)
    {
        using var request = CreateAuthorizedRequest(HttpMethod.Get, uri, accessToken);
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("OneDrive folder lookup starting. Operation: {GraphOperation}.", operationName);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        LogGraphResponse(operationName, response, stopwatch.ElapsedMilliseconds);
        if (response.IsSuccessStatusCode)
            return await ReadFolderIdAsync(response, cancellationToken);
        if (allowMissing && response.StatusCode == HttpStatusCode.NotFound)
            return null;
        await ThrowForGraphFailureAsync(response, operationName, cancellationToken);
        throw new InvalidOperationException("Graph failure handler returned unexpectedly.");
    }

    private static async Task<string> ReadFolderIdAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var item = document.RootElement;
            if (item.ValueKind == JsonValueKind.Object
                && item.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(id.GetString())
                && item.TryGetProperty("folder", out var folder) && folder.ValueKind == JsonValueKind.Object)
                return id.GetString()!;
        }
        catch (JsonException)
        {
            // Convert malformed successful responses into an actionable backup failure.
        }
        throw new OneDriveBackupException(OneDriveBackupFailureKind.Unknown,
            "OneDrive returned an invalid folder or a file where a backup folder was expected.");
    }

    private async Task UploadFileAsync(
        string accessToken,
        string localFilePath,
        string destinationFileName,
        string targetDirectoryName,
        string operationName,
        CancellationToken cancellationToken)
    {
        await using var fileStream = File.OpenRead(localFilePath);

        using var request = CreateAuthorizedRequest(
            HttpMethod.Put,
            BuildUploadEndpoint(targetDirectoryName, destinationFileName),
            accessToken);
        request.Content = new StreamContent(fileStream);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");

        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation(
            "OneDrive Graph request starting. Operation: {GraphOperation}. Method: {HttpMethod}. Relative URI: {RelativeUri}. Content length bytes: {ContentLengthBytes}.",
            operationName,
            request.Method.Method,
            request.RequestUri?.ToString(),
            fileStream.Length);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        LogGraphResponse(operationName, response, stopwatch.ElapsedMilliseconds);

        if (response.IsSuccessStatusCode)
            return;

        await ThrowForGraphFailureAsync(response, operationName, cancellationToken);
    }

    private async Task ThrowForGraphFailureAsync(
        HttpResponseMessage response,
        string operationName,
        CancellationToken cancellationToken)
    {
        var graphError = await ReadGraphErrorAsync(response, cancellationToken);
        var failureKind = ClassifyFailure(response.StatusCode, graphError?.Code);

        _logger.LogWarning(
            "OneDrive Graph request failed. Operation: {GraphOperation}. Status code: {StatusCode}. Graph error code: {GraphErrorCode}. Failure kind: {FailureKind}. Request id: {GraphRequestId}. Message: {GraphErrorMessage}.",
            operationName,
            (int)response.StatusCode,
            graphError?.Code,
            failureKind,
            GetHeaderValue(response, "request-id"), graphError?.Message);

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
        if (statusCode == HttpStatusCode.Unauthorized)
            return OneDriveBackupFailureKind.AuthRequired;

        if (statusCode == HttpStatusCode.Forbidden
            || string.Equals(graphErrorCode, "accessDenied", StringComparison.OrdinalIgnoreCase))
        {
            return OneDriveBackupFailureKind.AccessDenied;
        }

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
            OneDriveBackupFailureKind.AccessDenied => "OneDrive did not grant access to the app folder. Disconnect OneDrive, connect again, and retry.",
            OneDriveBackupFailureKind.NetworkUnavailable => "Unable to reach OneDrive right now. Check your connection and try again.",
            OneDriveBackupFailureKind.QuotaExceeded => "Your OneDrive storage is full. Free up space in OneDrive and try again.",
            _ => string.IsNullOrWhiteSpace(graphMessage)
                ? "The OneDrive backup failed."
                : $"The OneDrive backup failed: {graphMessage}"
        };
    }

    private void LogGraphResponse(string operationName, HttpResponseMessage response, long elapsedMilliseconds)
    {
        _logger.LogInformation(
            "OneDrive Graph request completed. Operation: {GraphOperation}. Status code: {StatusCode}. Elapsed milliseconds: {ElapsedMilliseconds}. Request id: {GraphRequestId}.",
            operationName,
            (int)response.StatusCode,
            elapsedMilliseconds,
            GetHeaderValue(response, "request-id"));
    }

    private static string BuildUploadEndpoint(string targetDirectoryName, string destinationFileName)
    {
        return $"me/drive/items/{Uri.EscapeDataString(targetDirectoryName)}:/{Uri.EscapeDataString(destinationFileName)}:/content";
    }

    private static string? GetHeaderValue(HttpResponseMessage response, string headerName)
    {
        return response.Headers.TryGetValues(headerName, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private string GetConnectionProfilesDisplay()
    {
        var connectionProfiles = _connectivity.ConnectionProfiles
            .Select(profile => profile.ToString())
            .OrderBy(profile => profile, StringComparer.Ordinal)
            .ToArray();

        return connectionProfiles.Length == 0
            ? "(none)"
            : string.Join(", ", connectionProfiles);
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
