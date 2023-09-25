// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using Aws.Ssm.Cli.Installer;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Installing aws-ssm-cli...\n");

var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var workingDirectory = InstallerHelper.GetBuildWorkingDirectory(configuration);
var output = InstallerHelper.GetBuildOutputDirectory(configuration);

var process = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    WorkingDirectory = workingDirectory,
    Arguments =
        $"publish src/Aws.Ssm.Cli/Aws.Ssm.Cli.csproj --output {output} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet /property:WarningLevel=0"
});

await process!.WaitForExitAsync();
