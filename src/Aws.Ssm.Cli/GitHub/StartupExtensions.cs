using Aws.Ssm.Cli.GitHub.Impl;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.Cli.GitHub;

public static class StartupExtensions
{
    public static IServiceCollection AddGitHubServices(
        this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddHttpClient<IGitHubClient, GitHubClientImpl>();

        return serviceCollection;
    }
}