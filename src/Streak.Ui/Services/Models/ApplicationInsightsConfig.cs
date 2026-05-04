namespace Streak.Ui.Services.Models;

public sealed class ApplicationInsightsConfig
{
    public string ConnectionString { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(ConnectionString);
}
