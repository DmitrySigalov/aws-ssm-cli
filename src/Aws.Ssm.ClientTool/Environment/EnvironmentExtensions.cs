using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Utils;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentExtensions
{
    public static IDictionary<string, string> SetEnvironmentVariables(
        this IEnvironmentVariablesRepository environmentVariablesRepository,
        IDictionary<string, string> ssmParameters,
        ProfileDo profileDo)
    {
        var result = new SortedDictionary<string, string>();

        foreach (var ssmParam in ssmParameters)
        {
            var envVarName = EnvironmentVariableNameConverter.ConvertFromSsmPath(ssmParam.Key, profileDo);
            
            environmentVariablesRepository.Set(envVarName, ssmParam.Value);
            
            result.Add(envVarName, ssmParam.Value);
        }
        
        return result;
    }
    
    public static IDictionary<string, string> DeleteEnvironmentVariables(
        this IEnvironmentVariablesRepository environmentVariablesRepository,
        ProfileDo profileDo)
    {
        var result = new SortedDictionary<string, string>();
        
        var convertedEnvironmentVariableBaseNames = profileDo.SsmPaths
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileDo))
            .ToArray();

        var environmentVariablesToDelete = environmentVariablesRepository
            .GetNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToDelete.Any() == false)
        {
            return result;
        }

        foreach (var envVarName in environmentVariablesToDelete)
        {
            var envVarValue = environmentVariablesRepository.Get(envVarName);
            
            environmentVariablesRepository.Delete(envVarName);
            
            result.Add(envVarName, envVarValue);
        }

        return result;
    }
    
    public static void PrintEnvironmentVariables(
        this IDictionary<string, string> environmentVariables,
        ProfileDo profileDo)
    {
        if (environmentVariables.Any() == false)
        {
            ConsoleUtils.WriteLineWarn("Environment variables empty list");
        }
        else
        {
            var table = new ConsoleTable("environment-variable-name", "value");

            foreach (var envVar in environmentVariables)
            {
                table.AddRow(envVar.Key, envVar.Value);
            }
        
            table.Write(Format.Minimal);
        }
        
        var invalidVariables = profileDo.SsmPaths
            .Distinct()
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileDo))
            .Where(x => environmentVariables.Keys.All(y => !y.StartsWith(x)))
            .ToArray();

        if (invalidVariables.Any() == true)
        {
            ConsoleUtils.Warn(() =>
            {
                var table = new ConsoleTable("missing-environment-variable_name");
                foreach (var envVar in invalidVariables.OrderBy(x => x))
                {
                    table.AddRow(envVar + "(*)");
                }

                table.Write(Format.Minimal);
            });
        }
    }
}