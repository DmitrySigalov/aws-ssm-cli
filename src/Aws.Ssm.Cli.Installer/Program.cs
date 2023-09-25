// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Runtime.InteropServices;
using Aws.Ssm.Cli.Installer;
using Microsoft.Extensions.Configuration;

Console.WriteLine("Installing aws-ssm-cli...");

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
    !RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
{
    throw new NotSupportedException($"Not supported {RuntimeInformation.RuntimeIdentifier}");
}


var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

var buildWorkingDirectory = InstallerHelper.GetBuildWorkingDirectory(configuration);
var appHomePath = InstallerHelper.GetAppHomeDirectory(configuration);

Console.WriteLine("Installing...");

var process = Process.Start(new ProcessStartInfo
{
    FileName = "dotnet",
    WorkingDirectory = buildWorkingDirectory,
    Arguments =
        $"publish src/Aws.Ssm.Cli/Aws.Ssm.Cli.csproj --output {appHomePath} --source https://api.nuget.org/v3/index.json --configuration Release --verbosity quiet /property:WarningLevel=0"
});

await process!.WaitForExitAsync();

Console.WriteLine("Adding app to machine path...");

