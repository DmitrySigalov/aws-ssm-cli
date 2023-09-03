using Aws.Ssm.ClientTool.Commands;
using Aws.Ssm.ClientTool.Commands.Handlers;
using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Environment.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.Extensions;

public static class StartupExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<CommandHandlerProvider>();

        serviceCollection
            .AddSingleton<HelpCommandHandler>()
            .AddSingleton<ICommandHandler, RunCommandHandler>()
            .AddSingleton<ICommandHandler, ViewCommandHandler>()
            .AddSingleton<ICommandHandler, ConfigCommandHandler>();

        return serviceCollection;
    }
    
    public static IServiceCollection AddEnvironmentBasedServices(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IEnvironmentVariablesRepository, DummyEnvironmentVariablesRepository>();
        
        return serviceCollection;
    }
}