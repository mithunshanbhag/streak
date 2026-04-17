using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

namespace Streak.Ui.Platforms.Android;

internal static class AndroidServiceProviderAccessor
{
    public static IServiceProvider GetRequiredServiceProvider()
    {
        return IPlatformApplication.Current?.Services
               ?? throw new InvalidOperationException("Unable to resolve the MAUI service provider for Android background work.");
    }
}
