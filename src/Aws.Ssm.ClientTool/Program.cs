using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Aws.Ssm.ClientTool.Commands;
using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) =>
{
    cts.Cancel();
    e.Cancel = true;
};

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
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
    .AddEnvironmentBasedServices()
    .AddSingleton<IProfilesRepository, ProfilesRepository>()
    .AddSingleton<ISsmParametersRepository, SsmParametersRepository>();

var serviceProvider = services.BuildServiceProvider();

try
{
    Console.WriteLine(Figgle.FiggleFonts.Standard.Render("Aws-Ssm-Cli"));
    
    var commandName = args.FirstOrDefault();

    var cliHandler = serviceProvider
        .GetRequiredService<CommandHandlerProvider>()
        .Get(commandName);

    await cliHandler.Handle(
        args.Skip(1).ToArray(), 
        cts.Token);
}
catch (Exception e)
{
    serviceProvider
        .GetRequiredService<ILogger<Program>>()
        .LogError(e, "An error has occurred");
}
