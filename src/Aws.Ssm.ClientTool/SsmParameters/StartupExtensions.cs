using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.SsmParameters;

public static class StartupExtensions
{
    public static IConfigurationBuilder AddAwsSsmConfiguration(this IConfigurationBuilder builder)
    {
        builder.AddSystemsManager(configurationSource =>
        {
            configurationSource.Path = "/";
            //configurationSource.ReloadAfter = TimeSpan.FromMinutes(15);
            configurationSource.ParameterProcessor = new SsmParameterProcessor();
        });

        return builder;
    }
    
    public static IServiceCollection AddSsmParametersRepository(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<SsmParametersRepository>();

        return serviceCollection;
    }
}