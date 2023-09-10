using System.Runtime.InteropServices;
using Aws.Ssm.Cli.EnvironmentVariables.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.Cli.EnvironmentVariables;

public static class StartupExtensions
{
    public static IServiceCollection AddEnvironmentVariablesServices(this IServiceCollection serviceCollection)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesProvider, WindowsEnvironmentVariablesProvider>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesProvider, OsxEnvironmentVariablesProvider>();
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
        
        return serviceCollection;
    }
}