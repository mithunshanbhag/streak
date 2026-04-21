namespace Streak.Core.Models.ViewModels.InputModels;

public sealed class CheckinProofInputModel
{
    #region Hidden Properties

    public required string ProofImageUri { get; set; }

    public required string ProofImageDisplayName { get; set; }

    public long ProofImageSizeBytes { get; set; }

    public required string ProofImageModifiedOn { get; set; }

    #endregion
}
