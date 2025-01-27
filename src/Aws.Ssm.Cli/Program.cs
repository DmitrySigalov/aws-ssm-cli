using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aws.Ssm.Cli.Commands;
using Aws.Ssm.Cli.EnvironmentVariables;
using Aws.Ssm.Cli.GitHub;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.Profiles.Services;
using Aws.Ssm.Cli.UserRuntime;
using Aws.Ssm.Cli.SsmParameters;
using Aws.Ssm.Cli.SsmParameters.Services;
using Aws.Ssm.Cli.VersionControl;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", false)
    .Build();

var services = new ServiceCollection();

services
    .AddSingleton<IConfiguration>(configuration)
    .AddLogging(builder =>
    {
        builder.ClearProviders();

        builder
            .SetMinimumLevel(LogLevel.Error)
            .AddSimpleConsole();
    })
    .AddUserRuntimeServices(args)
    .AddGitHubServices()
    .AddVersionControlServices();

services
    .AddCommandHandlers()
    .AddEnvironmentVariablesServices()
    .AddSingleton<IProfileConfigProvider, ProfileConfigProvider>()
    .AddSingleton<ISsmParametersProvider, SsmParametersProvider>();

var serviceProvider = services.BuildServiceProvider();

try
{
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Aws-Ssm-Cli"));

    var cliHandler = serviceProvider
        .GetRequiredService<CommandHandlerProvider>()
        .Get();
    
    await cliHandler.Handle(cts.Token);
}
catch (Exception e)
{
    serviceProvider
        .GetRequiredService<ILogger<Program>>()
        .LogError(e, "An error has occurred");
}
finally
{
    Thread.Sleep(1000);
    
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Goodbye"));
    
    await serviceProvider
        .GetRequiredService<IVersionControl>()
        .CheckVersionAsync(cts.Token);
}
