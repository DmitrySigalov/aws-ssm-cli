using Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;
using Aws.Ssm.ClientTool.EnvironmentVariables.Rules;
using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.Profiles;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.SsmParameters.Extensions;

public static class ConsoleOutputExtensions
{
    public static void PrintSsmParameters(
        this IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        if (ssmParameters.Any() == false)
        {
            ConsoleHelper.WriteLineWarn("Ssm parameters empty list");
        }
        else
        {
            var table = new ConsoleTable("ssm-parameter-name", "value");
            foreach (var ssmParam in ssmParameters.OrderBy(x => x.Key))
            {
                table.AddRow(ssmParam.Key, ssmParam.Value);
            }
            table.Write(Format.Minimal);
        }

        var invalidPaths = profileConfig.SsmPaths
            .Distinct()
            .Where(x => ssmParameters.Keys.All(y => !y.StartsWith(x)))
            .ToArray();

        if (invalidPaths.Any() == true)
        {
            ConsoleHelper.Error(() =>
            {
                var table = new ConsoleTable("ssm-path", "status");
                foreach (var ssmPath in invalidPaths.OrderBy(x => x))
                {
                    table.AddRow(ssmPath, "Unavailable");
                }

                table.Write(Format.Minimal);
            });
        }
    }
    
    public static void PrintSsmParameterToEnvironmentVariableNamesMapping(
        this IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        var mapping = ssmParameters
            .Select(x => new
            {
                SsmParameterName = x.Key,
                EnvironmentVariableName = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileConfig),
            })
            .ToDictionary(
                x => x.SsmParameterName,
                y => y.EnvironmentVariableName);;

        if (mapping.Any() == false)
        {
            return;
        }

        var table = new ConsoleTable("ssm-parameter-name", "environment-variable-name");

        foreach (var envVar in mapping)
        {
            table.AddRow(envVar.Key, envVar.Value);
        }

        table.Write(Format.Minimal);
    }

    private static IDictionary<string, string> GetSsmParameterNameToEnvironmentVariableNameMapping(
        this IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        return ssmParameters
            .Select(x => new
            {
                SsmParameterName = x.Key,
                EnvironmentVariableName = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileConfig),
            })
            .ToDictionary(
                x => x.SsmParameterName,
                y => y.EnvironmentVariableName);
    }
}