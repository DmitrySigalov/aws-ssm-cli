using Aws.Ssm.ClientTool.EnvironmentVariables.Rules;
using Aws.Ssm.ClientTool.Profiles;

namespace Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;

public static class EnvironmentVariablesSynchronizationExtensions
{
    public static IDictionary<string, string> GetEnvironmentVariablesWithInvalidSynchronizationStatus(
        this IDictionary<string, string> environmentVariables,
        IDictionary<string, string> ssmParameters,
        ProfileConfig profileConfig)
    {
        var ssmConvertedNamesValues = ssmParameters
            .Select(x => new
            {
                SsmParameterName = x.Key,
                EnvironmentVariableName = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileConfig),
                x.Value,
            })
            .ToArray();

        var result = ssmConvertedNamesValues
            .Where(x => environmentVariables.ContainsKey(x.EnvironmentVariableName))
            .Where(x => x.Value != environmentVariables[x.EnvironmentVariableName])
            .Select(x => new
            {
                Name = x.EnvironmentVariableName,
                Status = "NotEqual",
            })
            .ToHashSet();
        
        result.UnionWith(
            ssmConvertedNamesValues
                .Where(x => !environmentVariables.ContainsKey(x.EnvironmentVariableName))
                .Select(x => new
                {
                    Name = x.EnvironmentVariableName,
                    Status = "Unavailable",
                }));
        
        result.UnionWith(
            profileConfig.SsmPaths
                .Distinct()
                .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileConfig))
                .Where(x => environmentVariables.Keys.All(y => !y.StartsWith(x)))
                .Select(x => new
                {
                    Name = x + "(*)",
                    Status = "MissingSsmParameters",
                }));
        
        result.UnionWith(
            ssmConvertedNamesValues
                .GroupBy(x => x.EnvironmentVariableName)
                .Where(x => x.Count() > 1)
                .Select(x => new
                {
                    Name = x.Key,
                    Status = $"Mapped to {x.Count()} ssm parameters",
                }));
        
        return result
            .GroupBy(x => x.Name)
            .ToDictionary(
                x => x.Key,
                y => string.Join(",", y.Select(x => x.Status)));
    }
}