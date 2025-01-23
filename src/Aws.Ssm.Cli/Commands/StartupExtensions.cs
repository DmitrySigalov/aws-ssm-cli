using Aws.Ssm.Cli.Commands.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.Cli.Commands;

public static class StartupExtensions
{
    public static IServiceCollection AddCommandHandlers(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<CommandHandlerProvider>();

        serviceCollection
            .AddSingleton<HelpCommandHandler>()
            // First after help command is default
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, GetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, CleanEnvCommandHandler>()
            .AddSingleton<ICommandHandler, ViewProfileHandler>()
            .AddSingleton<ICommandHandler, ConfigProfileCommandHandler>();

        return serviceCollection;
    }
}