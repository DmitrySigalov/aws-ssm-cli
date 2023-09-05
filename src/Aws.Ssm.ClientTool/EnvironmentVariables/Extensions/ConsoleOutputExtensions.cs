using Aws.Ssm.ClientTool.EnvironmentVariables.Rules;
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
        
        var ssmConvertedNamesValues = ssmParameters
            .Select(x => new
            {
                Name = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileConfig),
                x.Value,
            })
            .ToArray();

        var notSynchronizedEnvVars = ssmConvertedNamesValues
            .Where(x => environmentVariables.ContainsKey(x.Name))
            .Where(x => x.Value != environmentVariables[x.Name])
            .Select(x => new
            {
                Name = x.Name,
                Status = "NotEqual",
            })
            .ToHashSet();
        
        notSynchronizedEnvVars.UnionWith(
            ssmConvertedNamesValues
                    .Where(x => !environmentVariables.ContainsKey(x.Name))
                    .Select(x => new
                    {
                        Name = x.Name,
                        Status = "Unavailable",
                    }));

        if (notSynchronizedEnvVars.Any())
        {
            var table = new ConsoleTable("environment-variable_name", "synchronization-status");

            foreach (var invalidEnvVar in notSynchronizedEnvVars.OrderBy(x => x.Name))
            {
                table.AddRow(invalidEnvVar.Name, invalidEnvVar.Status);
            }

            ConsoleHelper.Warn(() => table.Write(Format.Minimal));
        }
    }

    public static void PrintEnvironmentVariablesWithProfileValidation(
        this IDictionary<string, string> environmentVariables,
        ProfileConfig profileConfig)
    {
        PrintEnvironmentVariables(environmentVariables);
        
        var invalidVariables = profileConfig.SsmPaths
            .Distinct()
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileConfig))
            .Where(x => environmentVariables.Keys.All(y => !y.StartsWith(x)))
            .ToArray();

        if (invalidVariables.Any() == true)
        {
            ConsoleHelper.Warn(() =>
            {
                var table = new ConsoleTable("environment-variable-name", "synchronization-status");
                foreach (var envVar in invalidVariables.OrderBy(x => x))
                {
                    table.AddRow(envVar + "(*)", "Unavailable");
                }

                table.Write(Format.Minimal);
            });
        }
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
}