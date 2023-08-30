using Microsoft.Extensions.DependencyInjection;
using Ssm.Aws.ClientTool.Commands.Handlers;

namespace Ssm.Aws.ClientTool.Commands;

public static class StartupExtensions
{
    public static IServiceCollection AddCliCommands(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<CommandHandlerProvider>();

        serviceCollection
            .AddSingleton<ICommandHandler, ConfigCommandHandler>()
            .AddSingleton<ICommandHandler, RunCommandHandler>()
            .AddSingleton<ICommandHandler, ViewCommandHandler>();

        return serviceCollection;
    }
}