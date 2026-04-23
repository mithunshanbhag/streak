namespace Streak.Ui.UnitTests.Misc.Utilities;

public sealed class CheckinProofMediaStorePathUtilityTests
{
    #region Positive tests

    [Fact]
    public void TryBuildRelativeProofPath_ShouldNormalizeNestedMediaStorePath()
    {
        var result = CheckinProofMediaStorePathUtility.TryBuildRelativeProofPath(
            "Pictures/Streak/CheckinProofs",
            "Pictures\\Streak\\CheckinProofs\\Habit-5\\2026\\04\\2026-04-22\\",
            "proof.jpg",
            out var relativeProofPath);

        result.Should().BeTrue();
        relativeProofPath.Should().Be("Habit-5/2026/04/2026-04-22/proof.jpg");
    }

    [Fact]
    public void TryBuildRelativeProofPath_ShouldCollapseDuplicateSeparators()
    {
        var result = CheckinProofMediaStorePathUtility.TryBuildRelativeProofPath(
            "Pictures/Streak/CheckinProofs",
            "Pictures/Streak/CheckinProofs//Habit-1/2026/04/2026-04-23//",
            "proof.jpg",
            out var relativeProofPath);

        result.Should().BeTrue();
        relativeProofPath.Should().Be("Habit-1/2026/04/2026-04-23/proof.jpg");
    }

    [Fact]
    public void TryBuildRelativeProofPath_ShouldSupportFilesStoredDirectlyUnderProofRoot()
    {
        var result = CheckinProofMediaStorePathUtility.TryBuildRelativeProofPath(
            "Pictures/Streak/CheckinProofs",
            "Pictures/Streak/CheckinProofs/",
            "proof.jpg",
            out var relativeProofPath);

        result.Should().BeTrue();
        relativeProofPath.Should().Be("proof.jpg");
    }

    #endregion

    #region Negative tests

    [Fact]
    public void TryBuildRelativeProofPath_ShouldRejectFilesOutsideProofRoot()
    {
        var result = CheckinProofMediaStorePathUtility.TryBuildRelativeProofPath(
            "Pictures/Streak/CheckinProofs",
            "Pictures/Streak/SomethingElse/Habit-1/2026/04/2026-04-23/",
            "proof.jpg",
            out var relativeProofPath);

        result.Should().BeFalse();
        relativeProofPath.Should().BeEmpty();
    }

    #endregion
}
