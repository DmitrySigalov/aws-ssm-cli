using System.Runtime.CompilerServices;
using Aws.Ssm.Cli.EnvironmentVariables.Rules;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.EnvironmentVariables.Extensions;
using ConsoleTables;

namespace Aws.Ssm.Cli.SsmParameters.Extensions;

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
        
        ssmParameters.PrintInvalidSsmParameters(
            profileConfig);
    }

    public static void PrintInvalidSsmParameters(
        this IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
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
        
        Console.WriteLine("env:");
        foreach (var envVar in mapping)
        {
            Console.WriteLine($"  {envVar.Value}: '{{{{ssm \"{envVar.Key}\" \"region=eu-west-1\"}}}}'");
        }
        Console.WriteLine();
        
        // var table = new ConsoleTable("ssm-parameter-name", "environment-variable-name");
        //
        // foreach (var envVar in mapping)
        // {
        //     table.AddRow(envVar.Key, envVar.Value);
        // }
        //
        // table.Write(Format.Minimal);
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