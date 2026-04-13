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
    public void Settings_ShouldRenderOnlyDatabaseBackupContent()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        RegisterSettingsServices(exportServiceMock);

        var cut = RenderSettings();

        cut.Markup.Should().Contain("Backup");
        cut.Markup.Should().Contain("Download DB");
        cut.Markup.Should().Contain("Save a copy of your local data.");
        cut.Find("button[aria-label='Backup save location information']");
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

        RegisterSettingsServices(exportServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Export database']").Click();
        await exportStarted.Task;

        cut.WaitForAssertion(() =>
        {
            var button = cut.Find("button[aria-label='Export database']");
            button.HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowExportToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            var button = cut.Find("button[aria-label='Export database']");
            button.HasAttribute("disabled").Should().BeFalse();
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

        RegisterSettingsServices(exportServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Export database']").ClickAsync(new MouseEventArgs());

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

        RegisterSettingsServices(exportServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Export database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to export your database right now. Please try again."); });

        await cut.Find("button[aria-label='Export database']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your database right now. Please try again."); });
    }

    #endregion

    #region Private Helper Methods

    private void RegisterSettingsServices(Mock<IDatabaseExportService> exportServiceMock)
    {
        var importFilePickerMock = new Mock<IDatabaseImportFilePicker>();
        var importServiceMock = new Mock<IDatabaseImportService>();

        Services.AddSingleton(exportServiceMock.Object);
        Services.AddSingleton(importFilePickerMock.Object);
        Services.AddSingleton(importServiceMock.Object);
    }

    private IRenderedComponent<Settings> RenderSettings()
    {
        return RenderComponent<Settings>();
    }

    #endregion
}