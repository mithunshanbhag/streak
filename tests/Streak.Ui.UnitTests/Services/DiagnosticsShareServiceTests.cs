using System.IO.Compression;
using System.Text.Json;

namespace Streak.Ui.UnitTests.Services;

public sealed class DiagnosticsShareServiceTests
{
    #region Positive tests

    [Fact]
    public async Task ShareDiagnosticsAsync_ShouldCreateTimestampedBundleAndPassItToShareRequest()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        await File.WriteAllTextAsync(diagnosticsLogPath, """{"@t":"2026-04-17T00:00:00.0000000Z","@mt":"Sample log"}""");

        ShareFileRequest? shareRequest = null;
        string? inspectedBundlePath = null;

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns<ShareFileRequest>(request =>
            {
                shareRequest = request;
                inspectedBundlePath = Path.Combine(exportDirectory.Path, "inspected-diagnostics-share.zip");
                File.Copy(request.File.FullPath, inspectedBundlePath, true);

                return Task.CompletedTask;
            });

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            shareMock.Object);

        await sut.ShareDiagnosticsAsync();

        sut.CanShare.Should().BeTrue();
        shareRequest.Should().NotBeNull();
        inspectedBundlePath.Should().NotBeNull();

        var actualShareRequest = shareRequest!;

        actualShareRequest.Title.Should().Be("Share diagnostic logs");
        actualShareRequest.File.FullPath.Should().MatchRegex("^.+streak-diagnostics-[0-9]{8}-[0-9]{6}\\.zip$");
        File.Exists(actualShareRequest.File.FullPath).Should().BeTrue();
        File.Exists(inspectedBundlePath!).Should().BeTrue();

        using var zipArchive = ZipFile.OpenRead(inspectedBundlePath!);
        zipArchive.Entries.Select(x => x.FullName).Should().Contain(["logs/streak-diagnostics.log", "manifest.json"]);

        using var logReader = new StreamReader(zipArchive.GetEntry("logs/streak-diagnostics.log")!.Open());
        (await logReader.ReadToEndAsync()).Should().Contain("Sample log");

        using var manifestReader = new StreamReader(zipArchive.GetEntry("manifest.json")!.Open());
        var manifest = JsonDocument.Parse(await manifestReader.ReadToEndAsync());
        manifest.RootElement.GetProperty("structuredLogDirectory").GetString().Should().Be(diagnosticsDirectory.Path);
    }

    [Fact]
    public async Task ShareDiagnosticsAsync_ShouldStillShareBundle_WhenNoStructuredLogFileExists()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        string? inspectedBundlePath = null;

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns<ShareFileRequest>(request =>
            {
                inspectedBundlePath = Path.Combine(exportDirectory.Path, "inspected-empty-diagnostics-share.zip");
                File.Copy(request.File.FullPath, inspectedBundlePath, true);

                return Task.CompletedTask;
            });

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            shareMock.Object);

        await sut.ShareDiagnosticsAsync();

        File.Exists(inspectedBundlePath!).Should().BeTrue();

        using var zipArchive = ZipFile.OpenRead(inspectedBundlePath!);
        zipArchive.Entries.Select(x => x.FullName).Should().BeEquivalentTo(["manifest.json"]);
    }

    [Fact]
    public async Task ShareDiagnosticsAsync_ShouldDeleteOlderCachedBundlesBeforeCreatingANewOne()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        await File.WriteAllTextAsync(diagnosticsLogPath, "sample");

        var staleBundlePath = Path.Combine(exportDirectory.Path, "streak-diagnostics-20000101-010101.zip");
        File.WriteAllText(staleBundlePath, "stale");

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .Returns(Task.CompletedTask);

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            shareMock.Object);

        await sut.ShareDiagnosticsAsync();

        File.Exists(staleBundlePath).Should().BeFalse();
        Directory.GetFiles(exportDirectory.Path, "streak-diagnostics-*.zip").Should().HaveCount(1);
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task ShareDiagnosticsAsync_ShouldPropagateShareFailuresAndKeepBundleForRetry()
    {
        using var exportDirectory = new TemporaryDirectory();
        using var diagnosticsDirectory = new TemporaryDirectory();

        var diagnosticsLogPath = Path.Combine(diagnosticsDirectory.Path, DiagnosticsConstants.StructuredLogFileName);
        await File.WriteAllTextAsync(diagnosticsLogPath, "sample");

        var shareMock = new Mock<IShare>();
        shareMock
            .Setup(x => x.RequestAsync(It.IsAny<ShareFileRequest>()))
            .ThrowsAsync(new InvalidOperationException("Share failed."));

        var sut = CreateSut(
            exportDirectory.Path,
            diagnosticsDirectory.Path,
            shareMock.Object);

        var act = () => sut.ShareDiagnosticsAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Share failed.");

        Directory.GetFiles(exportDirectory.Path, "streak-diagnostics-*.zip").Should().HaveCount(1);
    }

    #endregion

    #region Private Helper Methods

    private static DiagnosticsShareService CreateSut(
        string exportDirectoryPath,
        string diagnosticsDirectoryPath,
        IShare share)
    {
        var appStoragePathServiceMock = new Mock<IAppStoragePathService>();
        appStoragePathServiceMock.SetupGet(x => x.ExportDirectoryPath).Returns(exportDirectoryPath);
        appStoragePathServiceMock.SetupGet(x => x.DiagnosticsDirectoryPath).Returns(diagnosticsDirectoryPath);

        var loggerMock = new Mock<ILogger<DiagnosticsShareService>>();

        return new DiagnosticsShareService(
            appStoragePathServiceMock.Object,
            TimeProvider.System,
            share,
            loggerMock.Object);
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
