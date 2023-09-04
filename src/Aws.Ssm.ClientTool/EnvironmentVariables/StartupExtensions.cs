using System.Runtime.InteropServices;
using Aws.Ssm.ClientTool.EnvironmentVariables.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.EnvironmentVariables;

public static class StartupExtensions
{
    public static IServiceCollection AddEnvironmentBasedServices(this IServiceCollection serviceCollection)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesProvider, DefaultEnvironmentVariablesProvider>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesProvider, MacEnvironmentVariablesProvider>();
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
        
        return serviceCollection;
    }
}