namespace Streak.Ui.Services.Models;

public sealed class CheckinDialogResult
{
    private CheckinDialogResult(string note, CheckinProofInputModel? proof)
    {
        Note = note;
        Proof = proof;
    }

    public string Note { get; }

    public CheckinProofInputModel? Proof { get; }

    public static CheckinDialogResult Create(string? note, CheckinProofInputModel? proof)
    {
        return new CheckinDialogResult(note ?? string.Empty, proof);
    }
}
