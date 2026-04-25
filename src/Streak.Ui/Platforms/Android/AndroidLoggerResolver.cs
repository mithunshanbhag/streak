using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android;

internal static class AndroidLoggerResolver
{
    public static string GetSafeDataScheme(string? scheme)
    {
        if (string.IsNullOrWhiteSpace(scheme))
            return "(none)";

        return scheme.StartsWith("msal", StringComparison.OrdinalIgnoreCase)
            ? "msal{client-id}"
            : scheme;
    }

    public static ILogger<T>? GetLogger<T>()
    {
        try
        {
            return AndroidServiceProviderAccessor
                .GetRequiredServiceProvider()
                .GetService<ILogger<T>>();
        }
        catch
        {
            return null;
        }
    }
}
