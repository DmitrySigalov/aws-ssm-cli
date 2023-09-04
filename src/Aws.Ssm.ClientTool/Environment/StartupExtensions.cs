using System.Runtime.InteropServices;
using Aws.Ssm.ClientTool.Environment.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.Environment;

public static class StartupExtensions
{
    public static IServiceCollection AddEnvironmentBasedServices(this IServiceCollection serviceCollection)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesRepository, DefaultEnvironmentVariablesRepository>();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesRepository, MacEnvironmentVariablesRepository>();
        }
        else
        {
            throw new NotSupportedException("OS not supported");
        }
        
        return serviceCollection;
    }
}