using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aws.Ssm.Cli.Commands;
using Aws.Ssm.Cli.EnvironmentVariables;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.Profiles.Services;
using Aws.Ssm.Cli.UserRuntime;
using Aws.Ssm.Cli.SsmParameters;
using Aws.Ssm.Cli.SsmParameters.Services;
using Okta.Aws.Cli.Abstractions;

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
    .AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddConsole();
    })
    .AddSingleton<IConfiguration>(configuration)
    .AddUserRuntimeServices(args)
    .AddVersionServices();

services
    .AddCommandHandlers()
    .AddEnvironmentVariablesServices()
    .AddSingleton<IProfileConfigProvider, ProfileConfigProvider>()
    .AddSingleton<ISsmParametersProvider, SsmParametersProvider>();

var serviceProvider = services.BuildServiceProvider();

try
{
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Aws-Ssm-Cli"));

    var versionService = serviceProvider
        .GetRequiredService<IVersionService>();
    var taskVersionService = versionService.CheckAsync(cts.Token);
    
    var cliHandler = serviceProvider
        .GetRequiredService<CommandHandlerProvider>()
        .Get();
    var taskCliHandler = cliHandler.Handle(cts.Token);
    
    await Task.WhenAll(taskVersionService, taskCliHandler);
}
catch (Exception e)
{
    serviceProvider
        .GetRequiredService<ILogger<Program>>()
        .LogError(e, "An error has occurred");
}
finally
{
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Goodbye"));
}
