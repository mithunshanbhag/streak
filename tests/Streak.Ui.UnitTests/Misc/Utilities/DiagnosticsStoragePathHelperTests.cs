namespace Streak.Ui.UnitTests.Misc.Utilities;

public sealed class DiagnosticsStoragePathHelperTests
{
    #region Positive tests

    [Fact]
    public void GetDiagnosticsDirectoryPath_ShouldAppendDiagnosticsDirectoryName()
    {
        var appDataDirectory = Path.Combine("C:\\", "temp", "streak-app-data");

        var result = DiagnosticsStoragePathHelper.GetDiagnosticsDirectoryPath(appDataDirectory);

        result.Should().Be(Path.Combine(appDataDirectory, DiagnosticsConstants.DiagnosticsDirectoryName));
    }

    [Fact]
    public void GetDiagnosticsLogFilePath_ShouldAppendStructuredLogFileName()
    {
        var appDataDirectory = Path.Combine("C:\\", "temp", "streak-app-data");

        var result = DiagnosticsStoragePathHelper.GetDiagnosticsLogFilePath(appDataDirectory);

        result.Should().Be(
            Path.Combine(
                appDataDirectory,
                DiagnosticsConstants.DiagnosticsDirectoryName,
                DiagnosticsConstants.StructuredLogFileName));
    }

    #endregion

    #region Negative tests

    [Fact]
    public void GetDiagnosticsDirectoryPath_ShouldThrow_WhenAppDataDirectoryIsBlank()
    {
        var act = () => DiagnosticsStoragePathHelper.GetDiagnosticsDirectoryPath(" ");

        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
