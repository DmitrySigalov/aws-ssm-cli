using Aws.Ssm.ClientTool.Environment.Repositories;
using Aws.Ssm.ClientTool.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.Environment;

public static class StartupExtensions
{
    public static IServiceCollection AddEnvironmentBasedServices(this IServiceCollection serviceCollection)
    {
        if (OperationSystemHelper.IsMacPlatform())
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesRepository, MacEnvironmentVariablesRepository>();
        }
        else
        {
            serviceCollection
                .AddSingleton<IEnvironmentVariablesRepository, DefaultEnvironmentVariablesRepository>();
        }
        
        return serviceCollection;
    }
}