using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles;
using ConsoleTables;

namespace Aws.Ssm.Cli.EnvironmentVariables.Extensions;

public static class ConsoleOutputExtensions
{
    public static void PrintEnvironmentVariablesWithSsmParametersValidationStatus(
        this IDictionary<string, string> environmentVariables,
        IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        environmentVariables.PrintEnvironmentVariables();

        environmentVariables.PrintInvalidEnvironmentVariables(
            ssmParameters,
            profileConfig);
    }

    public static void PrintInvalidEnvironmentVariables(
        this IDictionary<string, string> environmentVariables,
        IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        var invalidData = environmentVariables
            .GetEnvironmentVariablesWithInvalidSynchronizationStatus(
                ssmParameters,
                profileConfig);

        PrintInvalidEnvironmentVariables(invalidData);
    }
    
    private static void PrintEnvironmentVariables(
        this IDictionary<string, string> environmentVariables)
    {
        if (environmentVariables.Any() == false)
        {
            ConsoleHelper.WriteLineWarn("Environment variables empty list");

            return;
        }

        var table = new ConsoleTable("environment-variable-name", "value");

        foreach (var envVar in environmentVariables)
        {
            table.AddRow(envVar.Key, envVar.Value);
        }

        table.Write(Format.Minimal);
    }

    private static void PrintInvalidEnvironmentVariables(IDictionary<string, string> invalidData)
    {
        if (invalidData.Any() == false)
        {
            ConsoleHelper.WriteLineWarn($"Fully valid synchronized data");
            Console.WriteLine();
            return;
        }
        
        var table = new ConsoleTable("environment-variable-name", "synchronization-status");
        foreach (var envVar in invalidData.OrderBy(x => x.Key))
        {
            table.AddRow(envVar.Key, envVar.Value);
        }
        ConsoleHelper.Error(() => table.Write(Format.Minimal));
    }
}