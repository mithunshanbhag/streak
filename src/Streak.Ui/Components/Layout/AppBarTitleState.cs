namespace Streak.Ui.Components.Layout;

public sealed class AppBarTitleState
{
    private object? _currentOwner;

    public string TitleText { get; private set; } = string.Empty;

    public string? LeadingEmoji { get; private set; }

    public bool HasTitle => !string.IsNullOrWhiteSpace(TitleText);

    public bool HasLeadingEmoji => !string.IsNullOrWhiteSpace(LeadingEmoji);

    public event Action? Changed;

    public void SetTitle(object owner, string titleText, string? leadingEmoji = null)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(titleText);

        var normalizedLeadingEmoji = string.IsNullOrWhiteSpace(leadingEmoji)
            ? null
            : leadingEmoji;

        if (ReferenceEquals(_currentOwner, owner)
            && string.Equals(TitleText, titleText, StringComparison.Ordinal)
            && string.Equals(LeadingEmoji, normalizedLeadingEmoji, StringComparison.Ordinal))
        {
            return;
        }

        _currentOwner = owner;
        TitleText = titleText;
        LeadingEmoji = normalizedLeadingEmoji;
        Changed?.Invoke();
    }

    public void Clear(object owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        if (!ReferenceEquals(_currentOwner, owner))
            return;

        Reset();
    }

    public void Reset()
    {
        if (_currentOwner is null
            && string.IsNullOrEmpty(TitleText)
            && LeadingEmoji is null)
        {
            return;
        }

        _currentOwner = null;
        TitleText = string.Empty;
        LeadingEmoji = null;
        Changed?.Invoke();
    }
}
