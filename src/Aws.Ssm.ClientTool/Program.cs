using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aws.Ssm.ClientTool.Commands;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.UserSettings;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var configuration = new ConfigurationBuilder()
    //.SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false)
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
    .AddSingleton<UserSettingsRepository>()
    .AddSingleton<SsmParametersRepository>();

var serviceProvider = services.BuildServiceProvider();

try
{
    var commandName = args.FirstOrDefault();

    var cliHandler = serviceProvider
        .GetRequiredService<CommandHandlerProvider>()
        .Get(commandName);

    await cliHandler.Handle(cts.Token);
}
catch (Exception e)
{
    serviceProvider
        .GetRequiredService<ILogger<Program>>()
        .LogError(e, "An error has occurred");
}
