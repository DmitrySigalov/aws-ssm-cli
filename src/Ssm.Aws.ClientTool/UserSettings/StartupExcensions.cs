using Microsoft.Extensions.DependencyInjection;

namespace Ssm.Aws.ClientTool.UserSettings;

public static class StartupExcensions
{
    public static IServiceCollection AddUserSettings(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<UserSettingsRepository>();

        return serviceCollection;
    }
}