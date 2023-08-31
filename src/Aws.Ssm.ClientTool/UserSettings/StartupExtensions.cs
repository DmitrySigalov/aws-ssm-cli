using Microsoft.Extensions.DependencyInjection;

namespace Aws.Ssm.ClientTool.UserSettings;

public static class StartupExtensions
{
    public static IServiceCollection AddUserSettings(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<UserSettingsRepository>();

        return serviceCollection;
    }
}