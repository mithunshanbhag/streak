namespace Streak.Ui.Services.Models;

public sealed record OneDriveAuthConfiguration
{
    public required string ClientId { get; init; }

    public required IReadOnlyList<string> Scopes { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ClientId)
        && !string.Equals(ClientId, OneDriveAuthConstants.UnconfiguredClientId, StringComparison.OrdinalIgnoreCase);

    public string RedirectUri => $"msal{ClientId}://{OneDriveAuthConstants.RedirectUriHost}";
}
