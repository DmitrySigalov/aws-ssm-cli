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
            .AddSingleton<ICommandHandler, ViewProfileHandler>() // First is default
            .AddSingleton<ICommandHandler, GetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, SetEnvCommandHandler>()
            .AddSingleton<ICommandHandler, ConfigProfileCommandHandler>();

        return serviceCollection;
    }
}