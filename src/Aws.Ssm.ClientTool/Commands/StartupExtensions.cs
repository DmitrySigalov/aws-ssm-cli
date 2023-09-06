using Aws.Ssm.ClientTool.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.Commands;

public static class StartupExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<CommandHandlerProvider>();

        serviceCollection
            .AddSingleton<HelpCommandHandler>()
            .AddSingleton<ICommandHandler, JsonCommandHandler>() // First is default
            .AddSingleton<ICommandHandler, GetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, ConfigCommandHandler>();

        return serviceCollection;
    }
}