namespace Streak.Ui.UnitTests.Components.Pages;

public sealed class SettingsTests : TestContext
{
    public SettingsTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void Settings_ShouldRenderDataCardWithAutomatedBackupsBackupAndRestore()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        cut.Markup.Should().Contain("Daily automated backups");
        cut.Markup.Should().Contain("Create a nightly backup in local storage.");
        cut.Find("button[aria-label='Automated backup details']");
        cut.Find("input[type='checkbox']").HasAttribute("disabled").Should().BeTrue();
        cut.Markup.Should().Contain("Backup");
        cut.Markup.Should().Contain("Save or share a copy of your local data.");
        cut.Find("button[aria-label='Backup save location information']");
        cut.Find("button[aria-label='Download database']");
        cut.Find("button[aria-label='Share database']").HasAttribute("disabled").Should().BeTrue();
        cut.Markup.Should().Contain("Restore");
        cut.Markup.Should().Contain("Restore your data from a previous backup.");
        cut.Markup.Should().NotContain("Automated backups enabled");
        cut.Markup.Should().NotContain("Turns the nightly backup on or off.");
        cut.Markup.Should().NotContain("Daily reminder");
        cut.Markup.Should().NotContain("Create a manual backup of your local Streak data");
    }

    [Fact]
    public async Task Settings_ShouldDisableExportButtonWhileExportIsInProgress()
    {
        var exportStarted = new TaskCompletionSource();
        var allowExportToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                exportStarted.TrySetResult();
                await allowExportToFinish.Task;
                return DatabaseExportResult.Saved;
            });

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Download database']").Click();
        await exportStarted.Task;

        cut.WaitForAssertion(() =>
        {
            var button = cut.Find("button[aria-label='Download database']");
            button.HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowExportToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            var button = cut.Find("button[aria-label='Download database']");
            button.HasAttribute("disabled").Should().BeFalse();
        });
    }

    [Fact]
    public async Task Settings_ShouldEnableShareButtonAndInvokeShare_WhenSharingIsSupported()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: true);

        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Share database']").HasAttribute("disabled").Should().BeFalse();

        await cut.Find("button[aria-label='Share database']").ClickAsync(new MouseEventArgs());

        shareServiceMock.Verify(
            x => x.ShareDatabaseAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Settings_ShouldDisableExportAndImportWhileShareIsInProgress()
    {
        var shareStarted = new TaskCompletionSource();
        var allowShareToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: true);
        shareServiceMock
            .Setup(x => x.ShareDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                shareStarted.TrySetResult();
                await allowShareToFinish.Task;
            });

        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Share database']").Click();
        await shareStarted.Task;

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download database']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share database']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Import database']").HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowShareToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download database']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share database']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Import database']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task Settings_ShouldNotShowError_WhenExportIsCancelled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DatabaseExportResult.Cancelled);

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Download database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your database right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldShowErrorAndClearItOnRetry()
    {
        var attemptCount = 0;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                    throw new InvalidOperationException("Boom");

                return Task.FromResult(DatabaseExportResult.Saved);
            });

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Download database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to export your database right now. Please try again."); });

        await cut.Find("button[aria-label='Download database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your database right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldShowShareErrorAndClearItOnRetry()
    {
        var attemptCount = 0;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: true);
        shareServiceMock
            .Setup(x => x.ShareDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                    throw new InvalidOperationException("Boom");

                return Task.CompletedTask;
            });

        RegisterSettingsServices(exportServiceMock, shareServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Share database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to share your database right now. Please try again."); });

        await cut.Find("button[aria-label='Share database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to share your database right now. Please try again."); });
    }

    #endregion

    #region Private Helper Methods

    private static Mock<IDatabaseShareService> CreateShareServiceMock(bool canShare)
    {
        var shareServiceMock = new Mock<IDatabaseShareService>();
        shareServiceMock.SetupGet(x => x.CanShare).Returns(canShare);
        return shareServiceMock;
    }

    private void RegisterSettingsServices(
        Mock<IDatabaseExportService> exportServiceMock,
        Mock<IDatabaseShareService> shareServiceMock)
    {
        var importFilePickerMock = new Mock<IDatabaseImportFilePicker>();
        var importServiceMock = new Mock<IDatabaseImportService>();

        Services.AddSingleton(exportServiceMock.Object);
        Services.AddSingleton(shareServiceMock.Object);
        Services.AddSingleton(importFilePickerMock.Object);
        Services.AddSingleton(importServiceMock.Object);
    }

    private IRenderedComponent<Settings> RenderSettings()
    {
        return RenderComponent<Settings>();
    }

    #endregion
}
