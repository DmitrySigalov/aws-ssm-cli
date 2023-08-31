using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ssm.Aws.ClientTool.Commands;
using Ssm.Aws.ClientTool.UserSettings;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var configuration = new ConfigurationBuilder()
    //.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();

services
    .AddLogging(builder =>
    {
        builder.ClearProviders();
        builder.AddConsole();
    })
    .AddSingleton<IConfiguration>(configuration);

services
    .AddCommandHandlers()
    .AddUserSettings();

var provider = services.BuildServiceProvider();
var logger = provider.GetRequiredService<ILogger<Program>>();

try
{
    var commandName = args.FirstOrDefault();

    var cliHandler = provider
        .GetRequiredService<CommandHandlerProvider>()
        .Get(commandName);

    await cliHandler.Handle(cts.Token);
}
catch (Exception e)
{
    logger.LogError(e, "An error has occurred");
}
