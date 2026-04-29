namespace Streak.Ui.UnitTests.Components.Pages;

using Streak.Ui.Models.Storage;
using Streak.Ui.Models.ViewModels;
using Streak.Ui.Models.ViewModels.InputModels;
using Streak.Ui.Services.Interfaces;

public sealed class HomeTests : TestContext
{
    public HomeTests()
    {
        Services.AddMudServices(options => { options.PopoverOptions.CheckForPopoverProvider = false; });
        Services.AddLogging();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region Positive tests

    [Fact]
    public void Home_ShouldOpenCheckinNoteDialog_WhenUncheckedHabitIsChecked()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("Check in 'Exercise'");
            dialogProvider.Markup.Should().NotContain("Add an optional note or picture proof.");
            dialogProvider.Find("input[placeholder='Add a note (optional)']");
            dialogProvider.Markup.Should().NotContain("Picture proof (optional)");
            dialogProvider.Markup.Should().NotContain("One photo max.");
            dialogProvider.Markup.Should().Contain("Camera");
            dialogProvider.Markup.Should().Contain("Gallery");
            dialogProvider.Markup.Should().Contain("No picture selected");
            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeFalse();
        });

        checkinServiceMock.Verify(
            x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CheckinProofInputModel?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Home_ShouldRequestPostStartupPermissionRecovery_AfterFirstRender()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });
        var checkinProofServiceMock = CreateCheckinProofServiceMock();
        var postStartupPermissionRecoveryCoordinatorMock = CreatePostStartupPermissionRecoveryCoordinatorMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock, postStartupPermissionRecoveryCoordinatorMock);

        var cut = RenderComponent<Home>();

        cut.WaitForAssertion(() =>
        {
            postStartupPermissionRecoveryCoordinatorMock.Verify(
                x => x.RecoverMissingPermissionsAfterHomepageRenderAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }

    [Fact]
    public void Home_ShouldPersistOptionalNote_WhenCheckinIsConfirmed()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            },
            updatedStreak: 3);
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Find("input[placeholder='Add a note (optional)']"));
        dialogProvider.Find("input[placeholder='Add a note (optional)']").Input("30 mins cardio");
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Save check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync("Exercise", true, "30 mins cardio", null, It.IsAny<CancellationToken>()),
                Times.Once);

            cut.Markup.Should().Contain("3 day streak");
        });
    }

    [Fact]
    public void Home_ShouldShowPicturePreview_WhenPhotoIsChosen()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });
        var selectedProof = CreateCheckinProofSelection();
        var checkinProofServiceMock = CreateCheckinProofServiceMock(selectedPhoto: selectedProof);

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Gallery"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Gallery", StringComparison.Ordinal))
            .Click();

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain(selectedProof.DisplayName);
            dialogProvider.Markup.Should().Contain("Gallery");
            dialogProvider.Markup.Should().Contain("Replace");
            dialogProvider.Markup.Should().NotContain("Remove picture");
        });
    }

    [Fact]
    public void Home_ShouldPersistProofMetadata_WhenPhotoBackedCheckinIsConfirmed()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            },
            updatedStreak: 3);
        var selectedProof = CreateCheckinProofSelection();
        var persistedProof = CreateCheckinProofInputModel();
        var checkinProofServiceMock = CreateCheckinProofServiceMock(
            selectedPhoto: selectedProof,
            persistedProof: persistedProof);

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Gallery"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Gallery", StringComparison.Ordinal))
            .Click();

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain(selectedProof.DisplayName));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Save check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync(
                    "Exercise",
                    true,
                    string.Empty,
                    It.Is<CheckinProofInputModel?>(proof =>
                        proof != null &&
                        proof.ProofImageUri == persistedProof.ProofImageUri &&
                        proof.ProofImageDisplayName == persistedProof.ProofImageDisplayName &&
                        proof.ProofImageSizeBytes == persistedProof.ProofImageSizeBytes &&
                        proof.ProofImageModifiedOn == persistedProof.ProofImageModifiedOn),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        });
    }

    [Fact]
    public void Home_ShouldOpenRemoveCheckinDialog_WhenCheckedHabitIsUnchecked()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            },
            updatedStreak: 2);
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() =>
        {
            dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?");
            dialogProvider.Markup.Should().Contain("This also removes today's saved note and picture proof.");
            dialogProvider.Markup.Should().NotContain("Saved note");
            dialogProvider.Markup.Should().NotContain("Picture proof");
            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeTrue();
        });

        checkinServiceMock.Verify(
            x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CheckinProofInputModel?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public void Home_ShouldRemoveCheckin_WhenRemovalIsConfirmed()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            },
            updatedStreak: 2);
        var existingCheckin = new Checkin
        {
            HabitId = 1,
            CheckinDate = DateOnly.FromDateTime(DateTime.Now).ToString(CoreConstants.CheckinDateFormat, CultureInfo.InvariantCulture),
            ProofImageUri = "Habit-1/2026/04/2026-04-21/proof.jpg"
        };
        checkinServiceMock
            .Setup(x => x.GetByHabitNameAndDateAsync("Exercise", existingCheckin.CheckinDate, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingCheckin);
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Remove check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync("Exercise", false, null, null, It.IsAny<CancellationToken>()),
                Times.Once);
            checkinProofServiceMock.Verify(
                x => x.DeleteIfExistsAsync(existingCheckin.ProofImageUri, It.IsAny<CancellationToken>()),
                Times.Once);

            cut.Markup.Should().Contain("2 day streak");
        });
    }

    #endregion

    #region Negative tests

    [Fact]
    public void Home_ShouldNotPersistCheckin_WhenNoteDialogIsCancelled()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Check in 'Exercise'"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Cancel", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CheckinProofInputModel?>(), It.IsAny<CancellationToken>()),
                Times.Never);

            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeFalse();
            cut.Markup.Should().Contain("2 day streak");
        });
    }

    [Fact]
    public void Home_ShouldNotRemoveCheckin_WhenRemovalDialogIsCancelled()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            },
            updatedStreak: 2);
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Keep check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CheckinProofInputModel?>(), It.IsAny<CancellationToken>()),
                Times.Never);

            cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeTrue();
            cut.Markup.Should().Contain("3 day streak");
        });
    }

    [Fact]
    public void Home_ShouldOpenCheckinNoteDialogAgain_WhenRetryingAfterCancelledCheckin()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            });
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Check in 'Exercise'"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Cancel", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeFalse());

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Check in 'Exercise'"));
    }

    [Fact]
    public void Home_ShouldOpenRemoveCheckinDialogAgain_WhenRetryingAfterCancelledRemoval()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 3,
                IsDoneForToday = true
            });
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Keep check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() => cut.Find("input[type='checkbox']").HasAttribute("checked").Should().BeTrue());

        cut.Find("input[type='checkbox']").Change(false);

        dialogProvider.WaitForAssertion(() => dialogProvider.Markup.Should().Contain("Remove 'Exercise' check-in?"));
    }

    #endregion

    #region Boundary tests

    [Fact]
    public void Home_ShouldAllowCheckinWithoutOptionalNote()
    {
        var checkinServiceMock = CreateCheckinServiceMock(
            new HabitCheckinViewModel
            {
                HabitId = 1,
                HabitName = "Exercise",
                HabitEmoji = "💪",
                Streak = 2,
                IsDoneForToday = false
            },
            updatedStreak: 3);
        var checkinProofServiceMock = CreateCheckinProofServiceMock();

        RegisterServices(checkinServiceMock, checkinProofServiceMock);
        var dialogProvider = RenderComponent<MudDialogProvider>();
        var cut = RenderComponent<Home>();

        cut.Find("input[type='checkbox']").Change(true);

        dialogProvider.WaitForAssertion(() => dialogProvider.Find("input[placeholder='Add a note (optional)']"));
        dialogProvider.FindAll("button")
            .Single(x => x.TextContent.Contains("Save check-in", StringComparison.Ordinal))
            .Click();

        cut.WaitForAssertion(() =>
        {
            checkinServiceMock.Verify(
                x => x.ToggleForTodayAsync("Exercise", true, string.Empty, null, It.IsAny<CancellationToken>()),
                Times.Once);

            cut.Markup.Should().Contain("3 day streak");
        });
    }

    #endregion

    #region Private helper methods

    private static Mock<ICheckinService> CreateCheckinServiceMock(
        HabitCheckinViewModel habitCheckinViewModel,
        int updatedStreak = 0)
    {
        var checkinServiceMock = new Mock<ICheckinService>();
        checkinServiceMock
            .Setup(x => x.GetHomePageHabitCheckinsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([habitCheckinViewModel]);
        checkinServiceMock
            .Setup(x => x.GetCurrentStreakAsync(habitCheckinViewModel.HabitName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedStreak);
        checkinServiceMock
            .Setup(x => x.ToggleForTodayAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string?>(), It.IsAny<CheckinProofInputModel?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Checkin?)null);

        return checkinServiceMock;
    }

    private static Mock<ICheckinProofService> CreateCheckinProofServiceMock(
        bool supportsCameraCapture = true,
        CheckinProofSelection? selectedPhoto = null,
        CheckinProofInputModel? persistedProof = null)
    {
        var checkinProofServiceMock = new Mock<ICheckinProofService>();
        checkinProofServiceMock.SetupGet(x => x.SupportsCameraCapture).Returns(supportsCameraCapture);
        checkinProofServiceMock
            .Setup(x => x.CapturePhotoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(selectedPhoto);
        checkinProofServiceMock
            .Setup(x => x.PickPhotoAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(selectedPhoto);
        checkinProofServiceMock
            .Setup(x => x.PersistAsync(It.IsAny<CheckinProofSelection>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(persistedProof ?? CreateCheckinProofInputModel());
        checkinProofServiceMock
            .Setup(x => x.DeleteIfExistsAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return checkinProofServiceMock;
    }

    private static CheckinProofSelection CreateCheckinProofSelection()
    {
        return new CheckinProofSelection
        {
            DisplayName = "read-proof.jpg",
            FileBytes = [1, 2, 3, 4],
            FileExtension = ".jpg",
            ModifiedOn = "2026-04-21T08:30:12.0000000+05:30",
            PreviewDataUrl = "data:image/jpeg;base64,AQIDBA==",
            Source = CheckinProofSource.Gallery,
            SourceDescription = "Gallery"
        };
    }

    private static CheckinProofInputModel CreateCheckinProofInputModel()
    {
        return new CheckinProofInputModel
        {
            ProofImageUri = "Habit-1/2026/04/2026-04-21/proof.jpg",
            ProofImageDisplayName = "read-proof.jpg",
            ProofImageModifiedOn = "2026-04-21T08:30:12.0000000+05:30",
            ProofImageSizeBytes = 2048
        };
    }

    private void RegisterServices(
        Mock<ICheckinService> checkinServiceMock,
        Mock<ICheckinProofService> checkinProofServiceMock,
        Mock<IPostStartupPermissionRecoveryCoordinator>? postStartupPermissionRecoveryCoordinatorMock = null)
    {
        postStartupPermissionRecoveryCoordinatorMock ??= CreatePostStartupPermissionRecoveryCoordinatorMock();
        Services.AddSingleton(checkinServiceMock.Object);
        Services.AddSingleton(checkinProofServiceMock.Object);
        Services.AddSingleton(postStartupPermissionRecoveryCoordinatorMock.Object);
        Services.AddSingleton(TimeProvider.System);
    }

    private static Mock<IPostStartupPermissionRecoveryCoordinator> CreatePostStartupPermissionRecoveryCoordinatorMock()
    {
        var postStartupPermissionRecoveryCoordinatorMock = new Mock<IPostStartupPermissionRecoveryCoordinator>();
        postStartupPermissionRecoveryCoordinatorMock
            .Setup(x => x.RecoverMissingPermissionsAfterHomepageRenderAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return postStartupPermissionRecoveryCoordinatorMock;
    }

    #endregion
}
