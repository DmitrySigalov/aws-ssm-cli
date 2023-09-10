using Aws.Ssm.Cli.UserRuntime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.Cli.UserRuntime;

public static class StartupExtensions
{
    public static IServiceCollection AddUserRuntimeServices(
        this IServiceCollection serviceCollection,
        string[] args)
    {
        var userParameters = new UserParameters
        {
            CommandName = args.FirstOrDefault(x => !x.StartsWith("-")),
            Args = args,
        };

        serviceCollection
            .AddSingleton(userParameters)
            .AddSingleton<IUserFilesProvider, UserFilesProvider>();

        return serviceCollection;
    }
}