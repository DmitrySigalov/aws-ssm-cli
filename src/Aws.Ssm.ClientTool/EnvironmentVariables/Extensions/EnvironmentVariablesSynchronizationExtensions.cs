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
                Name = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, profileConfig),
                x.Value,
            })
            .ToArray();

        var result = ssmConvertedNamesValues
            .Where(x => environmentVariables.ContainsKey(x.Name))
            .Where(x => x.Value != environmentVariables[x.Name])
            .Select(x => new
            {
                Name = x.Name,
                Status = "NotEqual",
            })
            .ToHashSet();
        
        result.UnionWith(
            ssmConvertedNamesValues
                .Where(x => !environmentVariables.ContainsKey(x.Name))
                .Select(x => new
                {
                    Name = x.Name,
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
        
        return result
            .ToDictionary(
                x => x.Name,
                y => y.Status);
    }
}