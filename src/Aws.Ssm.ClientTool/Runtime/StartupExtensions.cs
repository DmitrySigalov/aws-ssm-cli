using Aws.Ssm.ClientTool.Runtime.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.Runtime;

public static class StartupExtensions
{
    public static IServiceCollection AddRuntimeServices(
        this IServiceCollection serviceCollection,
        string[] args)
    {
        var runtimeParameters = new RuntimeParameters
        {
            CommandName = args.FirstOrDefault(x => !x.StartsWith("-")),
            IsDebug = args.Contains("--debug"),
            Args = args,
        };

        serviceCollection
            .AddSingleton(runtimeParameters)
            .AddSingleton<IUserFilesProvider, UserFilesProvider>();

        return serviceCollection;
    }
}