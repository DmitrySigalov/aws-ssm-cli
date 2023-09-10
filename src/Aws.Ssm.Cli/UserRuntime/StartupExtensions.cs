using Aws.Ssm.Cli.UserRuntime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.Cli.UserRuntime;

public static class StartupExtensions
{
    public static IServiceCollection AddRuntimeServices(
        this IServiceCollection serviceCollection,
        string[] args)
    {
        var runtimeParameters = new UserParameters
        {
            CommandName = args.FirstOrDefault(x => !x.StartsWith("-")),
            Args = args,
        };

        serviceCollection
            .AddSingleton(runtimeParameters)
            .AddSingleton<IUserFilesProvider, UserFilesProvider>();

        return serviceCollection;
    }
}