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
            .AddSingleton<ICommandHandler, ConfigCommandHandler>()
            .AddSingleton<ICommandHandler, RunCommandHandler>()
            .AddSingleton<ICommandHandler, ViewCommandHandler>();

        return serviceCollection;
    }
}