using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aws.Ssm.ClientTool.Commands;
using Aws.Ssm.ClientTool.EnvironmentVariables;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Profiles.Services;
using Aws.Ssm.ClientTool.UserRuntime;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.SsmParameters.Services;

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
    .AddRuntimeServices(args);

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
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Goodbye"));
}
