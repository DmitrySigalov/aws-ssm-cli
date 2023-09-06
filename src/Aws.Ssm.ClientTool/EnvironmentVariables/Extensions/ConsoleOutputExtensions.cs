using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Helpers;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;

public static class ConsoleOutputExtensions
{
    public static void PrintEnvironmentVariablesWithSsmParametersValidation(
        this IDictionary<string, string> environmentVariables,
        IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        PrintEnvironmentVariables(environmentVariables);

        var invalidData = environmentVariables
            .GetEnvironmentVariablesWithInvalidSynchronizationStatus(
                ssmParameters,
                profileConfig);

        PrintInvalidEnvironmentVariables(invalidData);
    }

    public static void PrintEnvironmentVariablesWithProfileValidation(
        this IDictionary<string, string> environmentVariables,
        ProfileConfig profileConfig)
    {
        PrintEnvironmentVariables(environmentVariables);
        
        var invalidData = environmentVariables
            .GetEnvironmentVariablesWithMissingSsmParameters(profileConfig);

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
            ConsoleHelper.WriteLineWarn($"Fully synchronized data");
            return;
        }
        
        var table = new ConsoleTable("environment-variable-name", "synchronization-status");
        foreach (var envVar in invalidData.OrderBy(x => x.Key))
        {
            table.AddRow(envVar.Key, envVar.Value);
        }
        ConsoleHelper.Warn(() => table.Write(Format.Minimal));
    }
}