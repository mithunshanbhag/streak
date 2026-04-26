using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Streak.Ui.UnitTests.Services;

public sealed class AndroidOneDriveBackupUploadClientTests
{
    #region Positive tests

    [Fact]
    public async Task UploadManualBackupAsync_ShouldEnsureFolderHierarchyAndUploadArchive()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040200.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.OK, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var authServiceMock = CreateOneDriveAuthServiceMock("test-access-token");
        var sut = new OneDriveBackupUploadClient(
            httpClient,
            authServiceMock.Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        await sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        handler.Requests.Should().HaveCount(4);
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot");
        handler.Requests[1].Method.Should().Be(HttpMethod.Post);
        handler.Requests[1].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot/children");
        handler.Requests[2].Method.Should().Be(HttpMethod.Post);
        handler.Requests[2].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot:/Backups:/children");
        handler.Requests[3].Method.Should().Be(HttpMethod.Put);
        handler.Requests[3].RequestUri!.ToString().Should().Be($"https://graph.microsoft.com/v1.0/me/drive/special/approot:/Backups/Manual/{Path.GetFileName(archivePath)}:/content");
        handler.Requests.Should().OnlyContain(request =>
            request.Headers.Authorization != null
            && request.Headers.Authorization.Scheme == "Bearer"
            && request.Headers.Authorization.Parameter == "test-access-token");
    }

    [Fact]
    public async Task UploadAutomatedBackupAsync_ShouldEnsureFolderHierarchyAndUploadArchive()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-auto-data-backup-20260426-040250.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.OK, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var authServiceMock = CreateOneDriveAuthServiceMock("test-access-token");
        var sut = new OneDriveBackupUploadClient(
            httpClient,
            authServiceMock.Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        await sut.UploadAutomatedBackupAsync(archivePath, Path.GetFileName(archivePath));

        handler.Requests.Should().HaveCount(4);
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot");
        handler.Requests[1].Method.Should().Be(HttpMethod.Post);
        handler.Requests[1].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot/children");
        handler.Requests[2].Method.Should().Be(HttpMethod.Post);
        handler.Requests[2].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot:/Backups:/children");
        handler.Requests[3].Method.Should().Be(HttpMethod.Put);
        handler.Requests[3].RequestUri!.ToString().Should().Be($"https://graph.microsoft.com/v1.0/me/drive/special/approot:/Backups/Automated/{Path.GetFileName(archivePath)}:/content");
        handler.Requests.Should().OnlyContain(request =>
            request.Headers.Authorization != null
            && request.Headers.Authorization.Scheme == "Bearer"
            && request.Headers.Authorization.Parameter == "test-access-token");
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task UploadManualBackupAsync_ShouldThrowQuotaExceeded_WhenGraphReturnsQuotaFailure()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040300.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.OK, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Created, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.InsufficientStorage, "{\"error\":{\"code\":\"quotaLimitReached\",\"message\":\"Quota full.\"}}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var sut = new OneDriveBackupUploadClient(
            httpClient,
            CreateOneDriveAuthServiceMock("test-access-token").Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        Func<Task> act = () => sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.QuotaExceeded);
    }

    [Fact]
    public async Task UploadManualBackupAsync_ShouldThrowAccessDenied_WhenGraphDeniesAppFolderAccess()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040350.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.Forbidden, "{\"error\":{\"code\":\"accessDenied\",\"message\":\"Access denied.\"}}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var sut = new OneDriveBackupUploadClient(
            httpClient,
            CreateOneDriveAuthServiceMock("test-access-token").Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        Func<Task> act = () => sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.AccessDenied);
    }

    [Fact]
    public async Task UploadManualBackupAsync_ShouldThrowAuthRequired_WhenReconnectIsNeeded()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040400.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var authServiceMock = CreateOneDriveAuthServiceMock(accessToken: null);
        authServiceMock
            .Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveAuthenticationRequiredException("Reconnect required."));

        var sut = new OneDriveBackupUploadClient(
            httpClient,
            authServiceMock.Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        Func<Task> act = () => sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.AuthRequired);

        handler.Requests.Should().BeEmpty();
    }

    [Fact]
    public async Task UploadManualBackupAsync_ShouldThrowNetworkUnavailable_WhenGraphRequestFails()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040500.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([
            _ => throw new HttpRequestException("Network down.")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var sut = new OneDriveBackupUploadClient(
            httpClient,
            CreateOneDriveAuthServiceMock("test-access-token").Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        Func<Task> act = () => sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.NetworkUnavailable);
    }

    #endregion

    #region Private Helper Methods

    private static Mock<IOneDriveAuthService> CreateOneDriveAuthServiceMock(string? accessToken)
    {
        var authServiceMock = new Mock<IOneDriveAuthService>();
        authServiceMock
            .Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(accessToken ?? string.Empty);
        return authServiceMock;
    }

    private static HttpResponseMessage CreateJsonResponse(HttpStatusCode statusCode, string json)
    {
        return new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private sealed class SequenceHttpMessageHandler(IEnumerable<Func<HttpRequestMessage, HttpResponseMessage>> responders) : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responders = new(responders);

        public List<HttpRequestMessage> Requests { get; } = [];

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            if (_responders.Count == 0)
                throw new InvalidOperationException("No response was configured for the outgoing request.");

            return Task.FromResult(_responders.Dequeue().Invoke(request));
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"streak-ui-tests-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, true);
        }
    }

    #endregion
}
