namespace Streak.Ui.UnitTests.Components.Pages;

public sealed class SettingsTests : TestContext
{
    public SettingsTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        Services.AddLogging();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void Settings_ShouldRenderSettingsCards_InUpdatedOrder()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateDisconnectedOneDriveAuthState());
        var appVersionInfoServiceMock = CreateAppVersionInfoServiceMock();
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock,
            appVersionInfoServiceMock: appVersionInfoServiceMock);

        var cut = RenderSettings();
        var markup = cut.Markup;

        markup.Should().Contain("🔔 Daily reminder");
        markup.Should().Contain("You'll be reminded only if there are habits you haven't checked in yet.");
        cut.Find("input[aria-label='Daily reminder toggle']").HasAttribute("checked").Should().BeTrue();
        markup.Should().Contain("Reminder time");
        markup.Should().Contain("💾 Local backup");
        markup.Should().Contain("Create nightly local backups or manually save and share a copy of your data.");
        markup.Should().Contain("Daily automated backups");
        markup.Should().Contain("Create a nightly backup in local storage.");
        cut.Find("button[aria-label='Automated backup details']");
        cut.Find("input[aria-label='Daily automated backups toggle']").HasAttribute("disabled").Should().BeFalse();
        markup.Should().Contain("Manual backup");
        markup.Should().Contain("🌥️ Cloud backup");
        markup.Should().Contain("Optional OneDrive backups in your private app folder.");
        cut.Find("button[aria-label='Cloud backup details']");
        cut.Find("button[aria-label='Not connected. Connect OneDrive']").HasAttribute("disabled").Should().BeFalse();
        markup.Should().Contain("Personal Microsoft account");
        markup.Should().Contain("Not connected");
        markup.Should().NotContain("Tap the red cloud icon to connect.");
        cut.FindAll("input[aria-label='Daily automated cloud backup toggle']").Should().BeEmpty();
        cut.FindAll("button[aria-label='Back up to OneDrive']").Should().BeEmpty();
        markup.Should().NotContain("Manual cloud backup");
        markup.Should().NotContain("Daily automated cloud backup");
        markup.Should().Contain("Save or share a copy of your local data on this device.");
        cut.Find("button[aria-label='Manual backup save location info']");
        cut.Find("button[aria-label='Download data']");
        cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeTrue();
        markup.Should().Contain("↩️ Restore");
        markup.Should().Contain("🕵️ Diagnostic logs");
        markup.Should().Contain("Export or share a support bundle of recent app logs.");
        cut.Find("button[aria-label='Diagnostic log details']");
        cut.Find("button[aria-label='Export diagnostic logs']");
        cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
        markup.Should().Contain("Restore from a local .zip backup or a legacy .db file.");
        cut.Find("button[aria-label='Restore warning']");
        cut.Find("button[aria-label='Upload data']");
        markup.Should().NotContain(">Data<");
        markup.IndexOf("🔔 Daily reminder", StringComparison.Ordinal).Should().BeLessThan(markup.IndexOf("💾 Local backup", StringComparison.Ordinal));
        markup.IndexOf("💾 Local backup", StringComparison.Ordinal).Should().BeLessThan(markup.IndexOf("🌥️ Cloud backup", StringComparison.Ordinal));
        markup.IndexOf("🌥️ Cloud backup", StringComparison.Ordinal).Should().BeLessThan(markup.IndexOf("↩️ Restore", StringComparison.Ordinal));
        markup.IndexOf("↩️ Restore", StringComparison.Ordinal).Should().BeLessThan(markup.IndexOf("🕵️ Diagnostic logs", StringComparison.Ordinal));
    }

    [Fact]
    public void Settings_ShouldRenderVersionAndBuildBanner_AboveReminderCard()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var appVersionInfoServiceMock = CreateAppVersionInfoServiceMock("1.2.0", "456");

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock,
            appVersionInfoServiceMock: appVersionInfoServiceMock);

        var cut = RenderSettings();
        var markup = cut.Markup;
        var metadataBanner = cut.Find("[aria-label='App version and build']");

        metadataBanner.TextContent.Should().Contain("Version 1.2.0").And.Contain("Build 456");
        markup.Should().NotContain("mud-chip");
        markup.IndexOf("Version 1.2.0", StringComparison.Ordinal)
            .Should()
            .BeLessThan(markup.IndexOf("🔔 Daily reminder", StringComparison.Ordinal));
    }

    [Fact]
    public void Settings_ShouldRenderConnectedOneDriveState_WhenAuthStateIsConnected()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        cut.Markup.Should().Contain("streak-demo@outlook.com");
        cut.Find("button[aria-label='Connected. Disconnect OneDrive']").HasAttribute("disabled").Should().BeFalse();
        cut.Markup.Should().Contain("OneDrive connection");
        cut.Markup.Should().Contain("Signed in with your Microsoft account.");
        cut.Markup.Should().Contain("Manual cloud backup");
        cut.Find("button[aria-label='Manual cloud backup details']");
        cut.Markup.Should().Contain("Upload the latest backup to OneDrive.");
        cut.Find("button[aria-label='Back up to OneDrive']").HasAttribute("disabled").Should().BeFalse();
        cut.Markup.Should().Contain("Daily automated cloud backup");
        cut.Find("button[aria-label='Automated cloud backup details']");
        cut.Find("input[aria-label='Daily automated cloud backup toggle']").HasAttribute("disabled").Should().BeFalse();
        cut.Find("input[aria-label='Daily automated cloud backup toggle']").HasAttribute("checked").Should().BeFalse();
        cut.Markup.Should().Contain("Connected");
        cut.Markup.Should().Contain("Upload a nightly backup to OneDrive.");
        cut.Markup.Should().NotContain("Connected. Tap the green cloud icon to disconnect.");
        cut.Markup.Should().NotContain("Storage location");
        cut.Markup.Should().NotContain("Last backup");
    }

    [Fact]
    public void Settings_ShouldRenderManualBackupRecencyMetadata_WhenStoredStatusesExist()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());
        var localBackupCompletedAt = new DateTimeOffset(2026, 04, 26, 07, 05, 00, TimeSpan.Zero);
        var cloudBackupCompletedAt = new DateTimeOffset(2026, 04, 25, 22, 12, 00, TimeSpan.Zero);
        var manualBackupStatusStoreMock = CreateManualBackupStatusStoreMock(
            location => location switch
            {
                ManualBackupLocation.Local => localBackupCompletedAt,
                ManualBackupLocation.Cloud => cloudBackupCompletedAt,
                _ => null
            });

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock,
            manualBackupStatusStoreMock: manualBackupStatusStoreMock,
            timeProvider: new FixedTimeProvider(
                new DateTimeOffset(2026, 04, 26, 08, 00, 00, TimeSpan.Zero),
                TimeZoneInfo.Utc));

        var cut = RenderSettings();

        cut.Markup.Should().Contain("Last backup Today · 7:05 AM");
        cut.Markup.Should().Contain("Last backup Apr 25, 2026 · 10:12 PM");
    }

    [Fact]
    public async Task Settings_ShouldDisableExportButtonWhileExportIsInProgress()
    {
        var exportStarted = new TaskCompletionSource();
        var allowExportToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                exportStarted.TrySetResult();
                await allowExportToFinish.Task;
                return CreateSavedExportResult();
            });

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Download data']").Click();
        await exportStarted.Task;

        cut.WaitForAssertion(() =>
        {
            var button = cut.Find("button[aria-label='Download data']");
            button.HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowExportToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            var button = cut.Find("button[aria-label='Download data']");
            button.HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    [Fact]
    public async Task Settings_ShouldEnableShareButtonAndInvokeShare_WhenSharingIsSupported()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: true);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeFalse();

        await cut.Find("button[aria-label='Share data']").ClickAsync(new MouseEventArgs());

        shareServiceMock.Verify(
            x => x.ShareDatabaseAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Settings_ShouldEnableDiagnosticsShareButtonAndInvokeShare_WhenSharingIsSupported()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeFalse();

        await cut.Find("button[aria-label='Share diagnostic logs']").ClickAsync(new MouseEventArgs());

        diagnosticsShareServiceMock.Verify(
            x => x.ShareDiagnosticsAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Settings_ShouldNotifyManualBackupCompletion_WhenExportSucceeds()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var manualBackupCompletionNotifierMock = new Mock<IManualBackupCompletionNotifier>();

        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSavedExportResult());

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            CreateReminderConfigurationServiceMock(isEnabled: true),
            manualBackupCompletionNotifierMock: manualBackupCompletionNotifierMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Download data']").ClickAsync(new MouseEventArgs());

        manualBackupCompletionNotifierMock.Verify(
            x => x.NotifyCompleted(It.Is<DatabaseExportResult>(result =>
                result.Status == DatabaseExportStatus.Saved
                && result.SavedFileLocation != null
                && result.SavedFileLocation.ParentFolderDisplayPath == StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath)),
            Times.Once);
    }

    [Fact]
    public async Task Settings_ShouldConnectOneDrive_WhenInteractiveSignInSucceeds()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateDisconnectedOneDriveAuthState());
        oneDriveAuthServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OneDriveConnectResult.Connected(CreateConnectedOneDriveAuthState()));
        var oneDriveAuthReturnRouteStoreMock = new Mock<IOneDriveAuthReturnRouteStore>();

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            oneDriveAuthReturnRouteStoreMock: oneDriveAuthReturnRouteStoreMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Not connected. Connect OneDrive']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() =>
        {
            oneDriveAuthServiceMock.Verify(x => x.ConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
            oneDriveAuthReturnRouteStoreMock.Verify(
                x => x.SetPendingReturnRoute(RouteConstants.Settings),
                Times.Once);
            oneDriveAuthReturnRouteStoreMock.Verify(x => x.ClearPendingReturnRoute(), Times.Once);
            cut.Markup.Should().Contain("streak-demo@outlook.com");
            cut.Find("button[aria-label='Connected. Disconnect OneDrive']");
        });
    }

    [Fact]
    public async Task Settings_ShouldDisconnectOneDrive_WhenUserChoosesDisconnect()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());
        oneDriveAuthServiceMock
            .SetupSequence(x => x.GetAuthStateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateConnectedOneDriveAuthState())
            .ReturnsAsync(CreateDisconnectedOneDriveAuthState());

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Connected. Disconnect OneDrive']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() =>
        {
            oneDriveAuthServiceMock.Verify(x => x.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Once);
            cut.Find("button[aria-label='Not connected. Connect OneDrive']");
            cut.Markup.Should().Contain("Personal Microsoft account");
            cut.FindAll("button[aria-label='Back up to OneDrive']").Should().BeEmpty();
        });
    }

    [Fact]
    public async Task Settings_ShouldDisableExportAndImportWhileShareIsInProgress()
    {
        var shareStarted = new TaskCompletionSource();
        var allowShareToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        var shareServiceMock = CreateShareServiceMock(canShare: true);
        shareServiceMock
            .Setup(x => x.ShareDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                shareStarted.TrySetResult();
                await allowShareToFinish.Task;
            });

        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Share data']").Click();
        await shareStarted.Task;

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowShareToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    [Fact]
    public async Task Settings_ShouldUploadManualCloudBackupAndRefreshCloudRecencyMetadata()
    {
        DateTimeOffset? localBackupCompletedAt = null;
        DateTimeOffset? cloudBackupCompletedAt = null;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());
        var manualCloudBackupServiceMock = CreateManualCloudBackupServiceMock();
        manualCloudBackupServiceMock
            .Setup(x => x.UploadManualBackupAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                cloudBackupCompletedAt = new DateTimeOffset(2026, 04, 26, 07, 12, 00, TimeSpan.Zero);
                return Task.CompletedTask;
            });
        var manualBackupStatusStoreMock = CreateManualBackupStatusStoreMock(
            location => location switch
            {
                ManualBackupLocation.Local => localBackupCompletedAt,
                ManualBackupLocation.Cloud => cloudBackupCompletedAt,
                _ => null
            });

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            manualCloudBackupServiceMock: manualCloudBackupServiceMock,
            manualBackupStatusStoreMock: manualBackupStatusStoreMock,
            timeProvider: new FixedTimeProvider(
                new DateTimeOffset(2026, 04, 26, 08, 00, 00, TimeSpan.Zero),
                TimeZoneInfo.Utc));

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Back up to OneDrive']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() =>
        {
            manualCloudBackupServiceMock.Verify(x => x.UploadManualBackupAsync(It.IsAny<CancellationToken>()), Times.Once);
            cut.Markup.Should().Contain("Last backup Today · 7:12 AM");
        });
    }

    [Fact]
    public async Task Settings_ShouldDisableDataActionsWhileManualCloudBackupIsInProgress()
    {
        var cloudBackupStarted = new TaskCompletionSource();
        var allowCloudBackupToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: true);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());
        var manualCloudBackupServiceMock = CreateManualCloudBackupServiceMock();
        manualCloudBackupServiceMock
            .Setup(x => x.UploadManualBackupAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                cloudBackupStarted.TrySetResult();
                await allowCloudBackupToFinish.Task;
            });

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock,
            manualCloudBackupServiceMock: manualCloudBackupServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Back up to OneDrive']").Click();
        await cloudBackupStarted.Task;

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Back up to OneDrive']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Connected. Disconnect OneDrive']").HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowCloudBackupToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Back up to OneDrive']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Connected. Disconnect OneDrive']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    [Fact]
    public async Task Settings_ShouldDisableDataActionsWhileDiagnosticsExportIsInProgress()
    {
        var diagnosticsExportStarted = new TaskCompletionSource();
        var allowDiagnosticsExportToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        diagnosticsExportServiceMock
            .Setup(x => x.ExportDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                diagnosticsExportStarted.TrySetResult();
                await allowDiagnosticsExportToFinish.Task;
                return DiagnosticsExportResult.Saved;
            });

        var shareServiceMock = CreateShareServiceMock(canShare: true);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Export diagnostic logs']").Click();
        await diagnosticsExportStarted.Task;

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowDiagnosticsExportToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    [Fact]
    public async Task Settings_ShouldDisableDataActionsWhileDiagnosticsShareIsInProgress()
    {
        var diagnosticsShareStarted = new TaskCompletionSource();
        var allowDiagnosticsShareToFinish = new TaskCompletionSource();

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: true);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        diagnosticsShareServiceMock
            .Setup(x => x.ShareDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                diagnosticsShareStarted.TrySetResult();
                await allowDiagnosticsShareToFinish.Task;
            });

        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        cut.Find("button[aria-label='Share diagnostic logs']").Click();
        await diagnosticsShareStarted.Task;

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeTrue();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeTrue();
            cut.Markup.Should().Contain("mud-progress-circular");
        });

        allowDiagnosticsShareToFinish.TrySetResult();

        cut.WaitForAssertion(() =>
        {
            cut.Find("button[aria-label='Download data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Export diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share diagnostic logs']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Share data']").HasAttribute("disabled").Should().BeFalse();
            cut.Find("button[aria-label='Upload data']").HasAttribute("disabled").Should().BeFalse();
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public async Task Settings_ShouldNotShowError_WhenExportIsCancelled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DatabaseExportResult.Cancelled);

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Download data']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your data right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldShowErrorAndClearItOnRetry()
    {
        var attemptCount = 0;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        exportServiceMock
            .Setup(x => x.ExportDatabaseAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                    throw new InvalidOperationException("Boom");

                return Task.FromResult(CreateSavedExportResult());
            });

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Download data']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to export your data right now. Please try again."); });

        await cut.Find("button[aria-label='Download data']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your data right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldShowShareErrorAndClearItOnRetry()
    {
        var attemptCount = 0;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
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

        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Share data']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to share your data right now. Please try again."); });

        await cut.Find("button[aria-label='Share data']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to share your data right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldNotShowError_WhenDiagnosticsExportIsCancelled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        diagnosticsExportServiceMock
            .Setup(x => x.ExportDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(DiagnosticsExportResult.Cancelled);

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Export diagnostic logs']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your diagnostic logs right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldShowDiagnosticsExportErrorAndClearItOnRetry()
    {
        var attemptCount = 0;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        diagnosticsExportServiceMock
            .Setup(x => x.ExportDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                    throw new InvalidOperationException("Boom");

                return Task.FromResult(DiagnosticsExportResult.Saved);
            });

        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Export diagnostic logs']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to export your diagnostic logs right now. Please try again."); });

        await cut.Find("button[aria-label='Export diagnostic logs']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to export your diagnostic logs right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldShowDiagnosticsShareErrorAndClearItOnRetry()
    {
        var attemptCount = 0;

        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var diagnosticsShareServiceMock = CreateDiagnosticsShareServiceMock(canShare: true);
        diagnosticsShareServiceMock
            .Setup(x => x.ShareDiagnosticsAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                attemptCount++;
                if (attemptCount == 1)
                    throw new InvalidOperationException("Boom");

                return Task.CompletedTask;
            });

        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            diagnosticsShareServiceMock: diagnosticsShareServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Share diagnostic logs']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().Contain("Unable to share your diagnostic logs right now. Please try again."); });

        await cut.Find("button[aria-label='Share diagnostic logs']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() => { cut.Markup.Should().NotContain("Unable to share your diagnostic logs right now. Please try again."); });
    }

    [Fact]
    public async Task Settings_ShouldNotShowError_WhenOneDriveSignInIsCancelled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateDisconnectedOneDriveAuthState());
        oneDriveAuthServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OneDriveConnectResult.Cancelled(CreateDisconnectedOneDriveAuthState()));

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Not connected. Connect OneDrive']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().NotContain("Unable to connect OneDrive right now. Please try again.");
            cut.Find("button[aria-label='Not connected. Connect OneDrive']");
            cut.Markup.Should().Contain("Personal Microsoft account");
        });
    }

    [Fact]
    public async Task Settings_ShouldShowSpecificError_WhenManualCloudBackupRequiresReconnect()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());
        oneDriveAuthServiceMock
            .SetupSequence(x => x.GetAuthStateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateConnectedOneDriveAuthState())
            .ReturnsAsync(CreateConnectedOneDriveAuthState());

        var manualCloudBackupServiceMock = CreateManualCloudBackupServiceMock();
        manualCloudBackupServiceMock
            .Setup(x => x.UploadManualBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.AuthRequired,
                "Reconnect required."));

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            manualCloudBackupServiceMock: manualCloudBackupServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Back up to OneDrive']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("OneDrive needs you to reconnect before uploading. Disconnect and connect again, then retry.");
        });
    }

    [Fact]
    public async Task Settings_ShouldShowSpecificError_WhenManualCloudBackupAccessIsDenied()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());

        var manualCloudBackupServiceMock = CreateManualCloudBackupServiceMock();
        manualCloudBackupServiceMock
            .Setup(x => x.UploadManualBackupAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OneDriveBackupException(
                OneDriveBackupFailureKind.AccessDenied,
                "Access denied."));

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock,
            manualCloudBackupServiceMock: manualCloudBackupServiceMock);

        var cut = RenderSettings();

        await cut.Find("button[aria-label='Back up to OneDrive']").ClickAsync(new MouseEventArgs());

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("OneDrive did not grant access to the app folder. Disconnect and connect OneDrive again, then retry.");
        });
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void Settings_ShouldRenderAutomatedBackupToggleAsChecked_WhenPersistedStateIsEnabled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: true);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily automated backups toggle']").HasAttribute("checked").Should().BeTrue();
    }

    [Fact]
    public void Settings_ShouldHideReminderTimePicker_WhenRemindersAreDisabled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: false);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily reminder toggle']").HasAttribute("checked").Should().BeFalse();
        cut.Markup.Should().NotContain("Reminder time");
    }

    [Fact]
    public void Settings_ShouldDisableAutomatedBackupToggle_WhenFeatureIsUnsupported()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(
            isEnabled: true,
            isSupported: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();
        var toggle = cut.Find("input[aria-label='Daily automated backups toggle']");

        toggle.HasAttribute("disabled").Should().BeTrue();
        toggle.HasAttribute("checked").Should().BeFalse();
    }

    [Fact]
    public void Settings_ShouldDisableConnectButton_WhenOneDriveIsUnsupported()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateUnsupportedOneDriveAuthState());

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock);

        var cut = RenderSettings();
        var connectButton = cut.Find("button[aria-label='Unavailable']");

        cut.FindAll("input[aria-label='Daily automated cloud backup toggle']").Should().BeEmpty();
        connectButton.HasAttribute("disabled").Should().BeTrue();
        cut.Markup.Should().Contain("Cloud backup isn't supported on this platform in the current iteration.");
    }

    [Fact]
    public void Settings_ShouldDisableConnectButton_WhenOneDriveAuthIsNotConfigured()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateUnconfiguredOneDriveAuthState());

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock);

        var cut = RenderSettings();
        var connectButton = cut.Find("button[aria-label='Setup required']");

        connectButton.HasAttribute("disabled").Should().BeTrue();
        cut.Markup.Should().Contain("OneDrive sign-in isn't configured in this build yet.");
    }

    [Fact]
    public void Settings_ShouldPersistReminderToggle_WhenUserChangesIt()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily reminder toggle']").Change(false);

        reminderConfigurationServiceMock.Verify(x => x.SetIsEnabled(false), Times.Once);
    }

    [Fact]
    public void Settings_ShouldPersistAutomatedBackupToggle_WhenUserChangesIt()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock);

        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily automated backups toggle']").Change(true);

        backupConfigurationServiceMock.Verify(x => x.SetIsEnabled(true), Times.Once);
    }

    [Fact]
    public void Settings_ShouldPersistAutomatedCloudBackupToggle_WhenUserChangesIt()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false, isCloudEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var oneDriveAuthServiceMock = CreateOneDriveAuthServiceMock(CreateConnectedOneDriveAuthState());

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            oneDriveAuthServiceMock: oneDriveAuthServiceMock);

        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily automated cloud backup toggle']").Change(true);

        backupConfigurationServiceMock.Verify(x => x.SetIsCloudEnabled(true), Times.Once);
    }

    [Fact]
    public void Settings_ShouldShowSnackbar_WhenReminderNotificationPermissionIsDenied()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: false);
        var reminderNotificationPermissionCoordinatorMock = new Mock<IReminderNotificationPermissionCoordinator>();
        var snackbarMock = new Mock<ISnackbar>();
        reminderNotificationPermissionCoordinatorMock
            .Setup(x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            reminderNotificationPermissionCoordinatorMock: reminderNotificationPermissionCoordinatorMock,
            snackbarMock: snackbarMock);
        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily reminder toggle']").Change(true);

        cut.WaitForAssertion(() =>
        {
            reminderNotificationPermissionCoordinatorMock.Verify(
                x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            snackbarMock.Verify(
                x => x.Add(
                    It.Is<string>(message => message.Contains("Android reminder notifications are off.", StringComparison.Ordinal)),
                    Severity.Info,
                    It.IsAny<Action<SnackbarOptions>>(),
                    It.IsAny<string>()),
                Times.Once);
        });
    }

    [Fact]
    public void Settings_ShouldRequestReminderNotificationPermission_WhenRemindersAreAlreadyEnabled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var reminderNotificationPermissionCoordinatorMock = new Mock<IReminderNotificationPermissionCoordinator>();
        reminderNotificationPermissionCoordinatorMock
            .Setup(x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            reminderNotificationPermissionCoordinatorMock: reminderNotificationPermissionCoordinatorMock);

        var cut = RenderSettings();

        cut.WaitForAssertion(() =>
        {
            reminderNotificationPermissionCoordinatorMock.Verify(
                x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }

    [Fact]
    public void Settings_ShouldNotRequestReminderNotificationPermission_WhenRemindersAreDisabled()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: false);
        var reminderNotificationPermissionCoordinatorMock = new Mock<IReminderNotificationPermissionCoordinator>();
        reminderNotificationPermissionCoordinatorMock
            .Setup(x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            reminderNotificationPermissionCoordinatorMock: reminderNotificationPermissionCoordinatorMock);

        var cut = RenderSettings();

        cut.WaitForAssertion(() =>
        {
            reminderNotificationPermissionCoordinatorMock.Verify(
                x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()),
                Times.Never);
        });
    }

    [Fact]
    public void Settings_ShouldShowSnackbar_WhenAutomatedBackupNotificationPermissionIsDenied()
    {
        var exportServiceMock = new Mock<IDatabaseExportService>();
        var diagnosticsExportServiceMock = new Mock<IDiagnosticsExportService>();
        var shareServiceMock = CreateShareServiceMock(canShare: false);
        var backupConfigurationServiceMock = CreateBackupConfigurationServiceMock(isEnabled: false);
        var reminderConfigurationServiceMock = CreateReminderConfigurationServiceMock(isEnabled: true);
        var backupNotificationPermissionServiceMock = new Mock<IBackupNotificationPermissionService>();
        var snackbarMock = new Mock<ISnackbar>();
        backupNotificationPermissionServiceMock
            .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        RegisterSettingsServices(
            exportServiceMock,
            diagnosticsExportServiceMock,
            shareServiceMock,
            backupConfigurationServiceMock,
            reminderConfigurationServiceMock,
            backupNotificationPermissionServiceMock: backupNotificationPermissionServiceMock,
            snackbarMock: snackbarMock);
        var cut = RenderSettings();

        cut.Find("input[aria-label='Daily automated backups toggle']").Change(true);

        cut.WaitForAssertion(() =>
        {
            backupNotificationPermissionServiceMock.Verify(
                x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()),
                Times.Once);
            snackbarMock.Verify(
                x => x.Add(
                    It.Is<string>(message => message.Contains("Android backup notifications are off.", StringComparison.Ordinal)),
                    Severity.Info,
                    It.IsAny<Action<SnackbarOptions>>(),
                    It.IsAny<string>()),
                Times.Once);
        });
    }

    #endregion

    #region Private Helper Methods

    private static Mock<IDatabaseShareService> CreateShareServiceMock(bool canShare)
    {
        var shareServiceMock = new Mock<IDatabaseShareService>();
        shareServiceMock.SetupGet(x => x.CanShare).Returns(canShare);
        return shareServiceMock;
    }

    private static Mock<IDiagnosticsShareService> CreateDiagnosticsShareServiceMock(bool canShare)
    {
        var shareServiceMock = new Mock<IDiagnosticsShareService>();
        shareServiceMock.SetupGet(x => x.CanShare).Returns(canShare);
        return shareServiceMock;
    }

    private static Mock<IAutomatedBackupConfigurationService> CreateBackupConfigurationServiceMock(
        bool isEnabled,
        bool isCloudEnabled = false,
        bool isSupported = true)
    {
        var backupConfigurationServiceMock = new Mock<IAutomatedBackupConfigurationService>();
        backupConfigurationServiceMock.SetupGet(x => x.IsSupported).Returns(isSupported);
        backupConfigurationServiceMock.Setup(x => x.GetIsEnabled()).Returns(isEnabled);
        backupConfigurationServiceMock.Setup(x => x.GetIsCloudEnabled()).Returns(isCloudEnabled);
        backupConfigurationServiceMock.Setup(x => x.GetHasAnyEnabled()).Returns(isEnabled || isCloudEnabled);
        return backupConfigurationServiceMock;
    }

    private static Mock<IReminderConfigurationService> CreateReminderConfigurationServiceMock(
        bool isEnabled,
        TimeOnly? timeLocal = null)
    {
        var reminderConfigurationServiceMock = new Mock<IReminderConfigurationService>();
        reminderConfigurationServiceMock.Setup(x => x.GetIsEnabled()).Returns(isEnabled);
        reminderConfigurationServiceMock
            .Setup(x => x.GetTimeLocal())
            .Returns(timeLocal ?? new TimeOnly(21, 0));
        return reminderConfigurationServiceMock;
    }

    private static Mock<IAppVersionInfoService> CreateAppVersionInfoServiceMock(
        string displayVersion = "1.0",
        string buildNumber = "123")
    {
        var appVersionInfoServiceMock = new Mock<IAppVersionInfoService>();
        appVersionInfoServiceMock
            .Setup(x => x.GetCurrent())
            .Returns(new AppVersionInfo
            {
                DisplayVersion = displayVersion,
                BuildNumber = buildNumber
            });
        return appVersionInfoServiceMock;
    }

    private static Mock<IOneDriveAuthService> CreateOneDriveAuthServiceMock(OneDriveAuthState authState)
    {
        var oneDriveAuthServiceMock = new Mock<IOneDriveAuthService>();
        oneDriveAuthServiceMock
            .Setup(x => x.GetAuthStateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(authState);
        oneDriveAuthServiceMock
            .Setup(x => x.GetAccessTokenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("test-access-token");
        oneDriveAuthServiceMock
            .Setup(x => x.ConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(OneDriveConnectResult.Cancelled(authState));
        oneDriveAuthServiceMock
            .Setup(x => x.DisconnectAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return oneDriveAuthServiceMock;
    }

    private static Mock<IManualCloudBackupService> CreateManualCloudBackupServiceMock()
    {
        var manualCloudBackupServiceMock = new Mock<IManualCloudBackupService>();
        manualCloudBackupServiceMock
            .Setup(x => x.UploadManualBackupAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return manualCloudBackupServiceMock;
    }

    private static Mock<IManualBackupStatusStore> CreateManualBackupStatusStoreMock(
        Func<ManualBackupLocation, DateTimeOffset?>? getLastSuccessfulBackupUtc = null,
        Action<ManualBackupLocation, DateTimeOffset>? setLastSuccessfulBackupUtc = null)
    {
        var manualBackupStatusStoreMock = new Mock<IManualBackupStatusStore>();
        manualBackupStatusStoreMock
            .Setup(x => x.GetLastSuccessfulBackupUtc(It.IsAny<ManualBackupLocation>()))
            .Returns<ManualBackupLocation>(location => getLastSuccessfulBackupUtc?.Invoke(location));
        manualBackupStatusStoreMock
            .Setup(x => x.SetLastSuccessfulBackupUtc(It.IsAny<ManualBackupLocation>(), It.IsAny<DateTimeOffset>()))
            .Callback<ManualBackupLocation, DateTimeOffset>((location, timestamp) =>
                setLastSuccessfulBackupUtc?.Invoke(location, timestamp));
        return manualBackupStatusStoreMock;
    }

    private static OneDriveAuthState CreateConnectedOneDriveAuthState()
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = true,
            IsConnected = true,
            AccountUsername = "streak-demo@outlook.com"
        };
    }

    private static OneDriveAuthState CreateDisconnectedOneDriveAuthState()
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = true,
            IsConnected = false
        };
    }

    private static OneDriveAuthState CreateUnsupportedOneDriveAuthState()
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = false,
            IsConfigured = false,
            IsConnected = false
        };
    }

    private static OneDriveAuthState CreateUnconfiguredOneDriveAuthState()
    {
        return new OneDriveAuthState
        {
            IsPlatformSupported = true,
            IsConfigured = false,
            IsConnected = false
        };
    }

    private void RegisterSettingsServices(
        Mock<IDatabaseExportService> exportServiceMock,
        Mock<IDiagnosticsExportService> diagnosticsExportServiceMock,
        Mock<IDatabaseShareService> shareServiceMock,
        Mock<IAutomatedBackupConfigurationService> backupConfigurationServiceMock,
        Mock<IReminderConfigurationService> reminderConfigurationServiceMock,
        Mock<IOneDriveAuthService>? oneDriveAuthServiceMock = null,
        Mock<IManualCloudBackupService>? manualCloudBackupServiceMock = null,
        Mock<IManualBackupStatusStore>? manualBackupStatusStoreMock = null,
        Mock<IDiagnosticsShareService>? diagnosticsShareServiceMock = null,
        Mock<IManualBackupCompletionNotifier>? manualBackupCompletionNotifierMock = null,
        Mock<IBackupNotificationPermissionService>? backupNotificationPermissionServiceMock = null,
        Mock<IReminderNotificationPermissionCoordinator>? reminderNotificationPermissionCoordinatorMock = null,
        Mock<IAppVersionInfoService>? appVersionInfoServiceMock = null,
        Mock<IOneDriveAuthReturnRouteStore>? oneDriveAuthReturnRouteStoreMock = null,
        Mock<ISnackbar>? snackbarMock = null,
        TimeProvider? timeProvider = null)
    {
        var importFilePickerMock = new Mock<IDatabaseImportFilePicker>();
        var importServiceMock = new Mock<IDatabaseImportService>();
        manualBackupCompletionNotifierMock ??= new Mock<IManualBackupCompletionNotifier>();
        manualCloudBackupServiceMock ??= CreateManualCloudBackupServiceMock();
        manualBackupStatusStoreMock ??= CreateManualBackupStatusStoreMock();
        diagnosticsShareServiceMock ??= CreateDiagnosticsShareServiceMock(canShare: false);
        if (backupNotificationPermissionServiceMock is null)
        {
            backupNotificationPermissionServiceMock = new Mock<IBackupNotificationPermissionService>();
            backupNotificationPermissionServiceMock
                .Setup(x => x.RequestPermissionIfNeededAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }
        if (reminderNotificationPermissionCoordinatorMock is null)
        {
            reminderNotificationPermissionCoordinatorMock = new Mock<IReminderNotificationPermissionCoordinator>();
            reminderNotificationPermissionCoordinatorMock
                .Setup(x => x.RequestPermissionIfRemindersEnabledAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }
        appVersionInfoServiceMock ??= CreateAppVersionInfoServiceMock();
        oneDriveAuthServiceMock ??= CreateOneDriveAuthServiceMock(CreateDisconnectedOneDriveAuthState());
        oneDriveAuthReturnRouteStoreMock ??= new Mock<IOneDriveAuthReturnRouteStore>();
        snackbarMock ??= new Mock<ISnackbar>();
        timeProvider ??= TimeProvider.System;
        snackbarMock
            .Setup(x => x.Add(
                It.IsAny<string>(),
                It.IsAny<Severity>(),
                It.IsAny<Action<SnackbarOptions>>(),
                It.IsAny<string>()))
            .Returns((Snackbar?)null);

        Services.AddSingleton(backupConfigurationServiceMock.Object);
        Services.AddSingleton(exportServiceMock.Object);
        Services.AddSingleton(diagnosticsExportServiceMock.Object);
        Services.AddSingleton(shareServiceMock.Object);
        Services.AddSingleton(diagnosticsShareServiceMock.Object);
        Services.AddSingleton(importFilePickerMock.Object);
        Services.AddSingleton(importServiceMock.Object);
        Services.AddSingleton(manualBackupCompletionNotifierMock.Object);
        Services.AddSingleton(manualCloudBackupServiceMock.Object);
        Services.AddSingleton(manualBackupStatusStoreMock.Object);
        Services.AddSingleton(backupNotificationPermissionServiceMock.Object);
        Services.AddSingleton(reminderConfigurationServiceMock.Object);
        Services.AddSingleton(reminderNotificationPermissionCoordinatorMock.Object);
        Services.AddSingleton(oneDriveAuthServiceMock.Object);
        Services.AddSingleton(oneDriveAuthReturnRouteStoreMock.Object);
        Services.AddSingleton(appVersionInfoServiceMock.Object);
        Services.AddSingleton(snackbarMock.Object);
        Services.AddSingleton(timeProvider);
    }

    private static DatabaseExportResult CreateSavedExportResult()
    {
        return DatabaseExportResult.Saved(new SavedFileLocation
        {
            SavedFileDisplayPath = "Downloads/Streak/Backups/Manual/streak-data-backup-20260420-004200.zip",
            ParentFolderDisplayPath = StreakExportStorageConstants.ManualBackupsDisplayDirectoryPath
        });
    }

    private IRenderedComponent<Settings> RenderSettings()
    {
        return RenderComponent<Settings>();
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo localTimeZone) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow.ToUniversalTime();
        }

        public override TimeZoneInfo LocalTimeZone => localTimeZone;
    }

    #endregion
}
