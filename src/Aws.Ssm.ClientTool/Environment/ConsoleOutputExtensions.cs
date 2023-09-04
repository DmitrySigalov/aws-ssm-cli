using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Helpers;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.Environment;

public static class ConsoleOutputExtensions
{
    public static void PrintEnvironmentVariablesWithSsmParametersValidation(
        this IDictionary<string, string> environmentVariables,
        IDictionary<string, string> ssmParameters,
        ProfileDo profileDo)
    {
        PrintEnvironmentVariables(environmentVariables);
        
        var ssmConvertedNamesValues = ssmParameters
            .Select(x => new
            {
                Name = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileDo),
                Value = x.Value,
            })
            .ToDictionary(
                x => x.Name,
                y => y.Value);
        
        var notSynchronizedEnvVars = ssmConvertedNamesValues
            .Where(x => environmentVariables.ContainsKey(x.Key))
            .Where(x => x.Value != environmentVariables[x.Key])
            .Select(x => x.Key)
            .ToHashSet();

        notSynchronizedEnvVars.UnionWith(
            ssmConvertedNamesValues
                    .Keys
                    .Where(x => !environmentVariables.ContainsKey(x)));
        
        var table = new ConsoleTable("not-synchronized-environment-variable_name");

        foreach (var invalidEnvVar in notSynchronizedEnvVars.OrderBy(x => x))
        {
            table.AddRow(invalidEnvVar);
        }

        if (table.Rows.Any())
        {
            ConsoleHelper.Warn(() => table.Write(Format.Minimal));
        }
    }

    public static void PrintEnvironmentVariablesWithProfileValidation(
        this IDictionary<string, string> environmentVariables,
        ProfileDo profileDo)
    {
        PrintEnvironmentVariables(environmentVariables);
        
        var invalidVariables = profileDo.SsmPaths
            .Distinct()
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileDo))
            .Where(x => environmentVariables.Keys.All(y => !y.StartsWith(x)))
            .ToArray();

        if (invalidVariables.Any() == true)
        {
            ConsoleHelper.Warn(() =>
            {
                var table = new ConsoleTable("missing-environment-variable-name");
                foreach (var envVar in invalidVariables.OrderBy(x => x))
                {
                    table.AddRow(envVar + "(*)");
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