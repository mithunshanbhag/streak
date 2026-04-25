using Microsoft.Extensions.DependencyInjection;

namespace Streak.Ui.Platforms.Android;

internal static class AndroidLoggerResolver
{
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
