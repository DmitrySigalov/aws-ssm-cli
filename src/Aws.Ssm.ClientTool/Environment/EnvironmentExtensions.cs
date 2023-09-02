using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Utils;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentExtensions
{
    public static IDictionary<string, string> SetFromSsmParameters(
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
    
    public static IDictionary<string, string> GetAll(
        this IEnvironmentVariablesRepository environmentVariablesRepository,
        ProfileDo profileDo)
    {
        var result = new SortedDictionary<string, string>();
        
        var convertedEnvironmentVariableBaseNames = profileDo.SsmPaths
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileDo))
            .ToArray();

        var environmentVariablesToGet = environmentVariablesRepository
            .GetNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToGet.Any() == false)
        {
            return result;
        }

        foreach (var envVarName in environmentVariablesToGet)
        {
            var envVarValue = environmentVariablesRepository.Get(envVarName);
            
            result.Add(envVarName, envVarValue);
        }

        return result;
    }
    
    public static IDictionary<string, string> DeleteAll(
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
            ConsoleUtils.Warn(() => table.Write(Format.Minimal));
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
            ConsoleUtils.Warn(() =>
            {
                var table = new ConsoleTable("missing-environment-variable-name");
                foreach (var envVar in invalidVariables.OrderBy(x => x))
                {
                    table.AddRow(envVar + "(_*)");
                }

                table.Write(Format.Minimal);
            });
        }
    }

    public static void PrintEnvironmentVariables(
        this IDictionary<string, string> environmentVariables)
    {
        if (environmentVariables.Any() == false)
        {
            ConsoleUtils.WriteLineWarn("Environment variables empty list");

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