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
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"root\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var authServiceMock = CreateOneDriveAuthServiceMock("test-access-token");
        var sut = new OneDriveBackupUploadClient(
            httpClient,
            authServiceMock.Object,
            CreateConnectivityMock().Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        await sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        handler.Requests.Should().HaveCount(4);
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot");
        handler.Requests[1].Method.Should().Be(HttpMethod.Get);
        handler.Requests[1].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/items/root:/Backups");
        handler.Requests[2].Method.Should().Be(HttpMethod.Get);
        handler.Requests[2].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/items/folder:/Manual");
        handler.Requests[3].Method.Should().Be(HttpMethod.Put);
        handler.Requests[3].RequestUri!.ToString().Should().Be($"https://graph.microsoft.com/v1.0/me/drive/items/folder:/{Path.GetFileName(archivePath)}:/content");
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
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"root\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var authServiceMock = CreateOneDriveAuthServiceMock("test-access-token");
        var sut = new OneDriveBackupUploadClient(
            httpClient,
            authServiceMock.Object,
            CreateConnectivityMock().Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        await sut.UploadAutomatedBackupAsync(archivePath, Path.GetFileName(archivePath));

        handler.Requests.Should().HaveCount(4);
        handler.Requests[0].Method.Should().Be(HttpMethod.Get);
        handler.Requests[0].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/special/approot");
        handler.Requests[1].Method.Should().Be(HttpMethod.Get);
        handler.Requests[1].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/items/root:/Backups");
        handler.Requests[2].Method.Should().Be(HttpMethod.Get);
        handler.Requests[2].RequestUri!.ToString().Should().Be("https://graph.microsoft.com/v1.0/me/drive/items/folder:/Automated");
        handler.Requests[3].Method.Should().Be(HttpMethod.Put);
        handler.Requests[3].RequestUri!.ToString().Should().Be($"https://graph.microsoft.com/v1.0/me/drive/items/folder:/{Path.GetFileName(archivePath)}:/content");
        handler.Requests.Should().OnlyContain(request =>
            request.Headers.Authorization != null
            && request.Headers.Authorization.Scheme == "Bearer"
            && request.Headers.Authorization.Parameter == "test-access-token");
    }

    #endregion

    #region Negative tests

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Upload_ShouldCreateMissingFoldersAndResolveConflicts(bool automated, bool conflict)
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "backup.zip");
        await File.WriteAllTextAsync(path, "backup");
        var responses = new List<Func<HttpRequestMessage, HttpResponseMessage>>
        {
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"root\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.NotFound, "{}"),
            request =>
            {
                request.Method.Should().Be(HttpMethod.Post);
                request.RequestUri!.AbsolutePath.Should().EndWith("/items/root/children");
                var body = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
                using var json = System.Text.Json.JsonDocument.Parse(body);
                json.RootElement.GetProperty("name").GetString().Should().Be("Backups");
                json.RootElement.GetProperty("folder").ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
                json.RootElement.GetProperty("@microsoft.graph.conflictBehavior").GetString().Should().Be("fail");
                return CreateJsonResponse(conflict ? HttpStatusCode.Conflict : HttpStatusCode.Created,
                    "{\"id\":\"backups\",\"folder\":{}}");
            }
        };
        if (conflict)
            responses.Add(_ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"backups\",\"folder\":{}}"));
        responses.Add(request =>
        {
            request.RequestUri!.AbsolutePath.Should().EndWith(automated ? "/items/backups:/Automated" : "/items/backups:/Manual");
            return CreateJsonResponse(HttpStatusCode.NotFound, "{}");
        });
        responses.Add(request =>
        {
            request.RequestUri!.AbsolutePath.Should().EndWith("/items/backups/children");
            return CreateJsonResponse(HttpStatusCode.Created, "{\"id\":\"destination\",\"folder\":{}}");
        });
        responses.Add(request =>
        {
            request.Method.Should().Be(HttpMethod.Put);
            request.RequestUri!.AbsolutePath.Should().EndWith("/items/destination:/backup.zip:/content");
            return CreateJsonResponse(HttpStatusCode.Created, "{}");
        });
        var handler = new SequenceHttpMessageHandler(responses);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
        var sut = new OneDriveBackupUploadClient(client, CreateOneDriveAuthServiceMock("token").Object,
            CreateConnectivityMock().Object, new Mock<ILogger<OneDriveBackupUploadClient>>().Object);
        if (automated)
            await sut.UploadAutomatedBackupAsync(path, "backup.zip");
        else
            await sut.UploadManualBackupAsync(path, "backup.zip");
        handler.Requests.Should().HaveCount(conflict ? 7 : 6);
    }

    [Theory]
    [InlineData(400, OneDriveBackupFailureKind.Unknown)]
    [InlineData(403, OneDriveBackupFailureKind.AccessDenied)]
    [InlineData(429, OneDriveBackupFailureKind.NetworkUnavailable)]
    [InlineData(503, OneDriveBackupFailureKind.NetworkUnavailable)]
    public async Task Upload_ShouldStopOnFolderCreationFailure(int status, OneDriveBackupFailureKind kind)
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "backup.zip");
        await File.WriteAllTextAsync(path, "backup");
        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"root\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.NotFound, "{}"),
            _ => CreateJsonResponse((HttpStatusCode)status, "{\"error\":{\"code\":\"invalidRequest\",\"message\":\"Rejected\"}}")]);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
        var sut = new OneDriveBackupUploadClient(client, CreateOneDriveAuthServiceMock("token").Object,
            CreateConnectivityMock().Object, new Mock<ILogger<OneDriveBackupUploadClient>>().Object);
        Func<Task> act = () => sut.UploadAutomatedBackupAsync(path, "backup.zip");
        await act.Should().ThrowAsync<OneDriveBackupException>().Where(e => e.FailureKind == kind);
        handler.Requests.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"id\":\"file\",\"file\":{}}")]
    [InlineData("{\"id\":\"\",\"folder\":{}}")]
    [InlineData("not json")]
    public async Task Upload_ShouldRejectInvalidFolder(string body)
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "backup.zip");
        await File.WriteAllTextAsync(path, "backup");
        var handler = new SequenceHttpMessageHandler([_ => CreateJsonResponse(HttpStatusCode.OK, body)]);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
        var sut = new OneDriveBackupUploadClient(client, CreateOneDriveAuthServiceMock("token").Object,
            CreateConnectivityMock().Object, new Mock<ILogger<OneDriveBackupUploadClient>>().Object);
        Func<Task> act = () => sut.UploadAutomatedBackupAsync(path, "backup.zip");
        await act.Should().ThrowAsync<OneDriveBackupException>();
        handler.Requests.Should().HaveCount(1);
    }

    [Fact]
    public async Task Upload_ShouldPropagateCancellation()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "backup.zip");
        await File.WriteAllTextAsync(path, "backup");
        using var cancellation = new CancellationTokenSource();
        var handler = new SequenceHttpMessageHandler([_ =>
        {
            cancellation.Cancel();
            throw new TaskCanceledException();
        }]);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
        var sut = new OneDriveBackupUploadClient(client, CreateOneDriveAuthServiceMock("token").Object,
            CreateConnectivityMock().Object, new Mock<ILogger<OneDriveBackupUploadClient>>().Object);
        Func<Task> act = () => sut.UploadAutomatedBackupAsync(path, "backup.zip", cancellation.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Theory]
    [InlineData(200, "{\"id\":\"file\",\"file\":{}}")]
    [InlineData(200, "{}")]
    [InlineData(404, "{}")]
    [InlineData(403, "{}")]
    public async Task Upload_ShouldNotAcceptConflictWithoutValidFolder(int status, string body)
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "backup.zip");
        await File.WriteAllTextAsync(path, "backup");
        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"root\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.NotFound, "{}"),
            _ => CreateJsonResponse(HttpStatusCode.Conflict, "{}"),
            _ => CreateJsonResponse((HttpStatusCode)status, body)]);
        using var client = new HttpClient(handler) { BaseAddress = new Uri("https://graph.microsoft.com/v1.0/") };
        var sut = new OneDriveBackupUploadClient(client, CreateOneDriveAuthServiceMock("token").Object,
            CreateConnectivityMock().Object, new Mock<ILogger<OneDriveBackupUploadClient>>().Object);
        Func<Task> act = () => sut.UploadAutomatedBackupAsync(path, "backup.zip");
        await act.Should().ThrowAsync<OneDriveBackupException>();
        handler.Requests.Should().HaveCount(4);
        handler.Requests.Last().Method.Should().Be(HttpMethod.Get);
    }

    [Fact]
    public async Task UploadManualBackupAsync_ShouldThrowQuotaExceeded_WhenGraphReturnsQuotaFailure()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040300.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"root\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.OK, "{\"id\":\"folder\",\"folder\":{}}"),
            _ => CreateJsonResponse(HttpStatusCode.InsufficientStorage, "{\"error\":{\"code\":\"quotaLimitReached\",\"message\":\"Quota full.\"}}")
        ]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var sut = new OneDriveBackupUploadClient(
            httpClient,
            CreateOneDriveAuthServiceMock("test-access-token").Object,
            CreateConnectivityMock().Object,
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
            CreateConnectivityMock().Object,
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
            CreateConnectivityMock().Object,
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
            CreateConnectivityMock().Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        Func<Task> act = () => sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.NetworkUnavailable);
    }

    [Fact]
    public async Task UploadManualBackupAsync_ShouldThrowNetworkUnavailable_WhenAccessTokenAcquisitionFailsWithHttpRequestException()
    {
        using var temporaryDirectory = new TemporaryDirectory();

        var archivePath = Path.Combine(temporaryDirectory.Path, "streak-data-backup-20260426-040550.zip");
        await File.WriteAllTextAsync(archivePath, "backup");

        var handler = new SequenceHttpMessageHandler([]);
        using var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://graph.microsoft.com/v1.0/")
        };

        var authServiceMock = new Mock<IOneDriveAuthService>();
        authServiceMock
            .Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("DNS lookup failed."));

        var sut = new OneDriveBackupUploadClient(
            httpClient,
            authServiceMock.Object,
            CreateConnectivityMock(Microsoft.Maui.Networking.NetworkAccess.None).Object,
            new Mock<ILogger<OneDriveBackupUploadClient>>().Object);

        Func<Task> act = () => sut.UploadManualBackupAsync(archivePath, Path.GetFileName(archivePath));

        await act.Should().ThrowAsync<OneDriveBackupException>()
            .Where(exception => exception.FailureKind == OneDriveBackupFailureKind.NetworkUnavailable);

        handler.Requests.Should().BeEmpty();
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

    private static Mock<IConnectivity> CreateConnectivityMock(
        Microsoft.Maui.Networking.NetworkAccess networkAccess = Microsoft.Maui.Networking.NetworkAccess.Internet,
        params ConnectionProfile[] connectionProfiles)
    {
        var connectivityMock = new Mock<IConnectivity>();
        connectivityMock.SetupGet(x => x.NetworkAccess).Returns(networkAccess);
        connectivityMock
            .SetupGet(x => x.ConnectionProfiles)
            .Returns(connectionProfiles);
        return connectivityMock;
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
