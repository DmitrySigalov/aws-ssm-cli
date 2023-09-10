using Aws.Ssm.Cli.EnvironmentVariables.Extensions;
using Aws.Ssm.Cli.EnvironmentVariables.Rules;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles;
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
    }

    public static void PrintSsmParametersToEnvironmentVariables(
        this IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        var convertedEnvironmentVariables = ssmParameters
            .Select(x => new
            {
                Name = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileConfig),
                Value = x.Value,
            })
            .GroupBy(x => x.Name)
            .ToDictionary(
                x => x.Key,
                x => x.Last().Value);

        convertedEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidationStatus(
            ssmParameters,
            profileConfig);
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

}