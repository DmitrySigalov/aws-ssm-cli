using Microsoft.Extensions.DependencyInjection;
using Okta.Aws.Cli.Abstractions.Services;

namespace Okta.Aws.Cli.Abstractions;

public static class StartupExtensions
{
    public static IServiceCollection AddVersionServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<IVersionService, VersionService>();

        return serviceCollection;
    }
}