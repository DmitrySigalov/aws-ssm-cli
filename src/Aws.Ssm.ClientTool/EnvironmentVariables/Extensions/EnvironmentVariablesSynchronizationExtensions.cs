using Aws.Ssm.ClientTool.EnvironmentVariables.Rules;
using Aws.Ssm.ClientTool.Profiles;

namespace Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;

public static class EnvironmentVariablesSynchronizationExtensions
{
    public static IDictionary<string, string> GetNotSynchronizedNames(
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

        return result
            .ToDictionary(
                x => x.Name,
                y => y.Status);
    }

    public static IDictionary<string, string> GetNotSynchronizedNames(
        this IDictionary<string, string> environmentVariables,
        ProfileConfig profileConfig)
    {
        return profileConfig.SsmPaths
            .Distinct()
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, profileConfig))
            .Where(x => environmentVariables.Keys.All(y => !y.StartsWith(x)))
            .ToDictionary(
                x => x + "(*)",
                _ => "MissingSsmPath");
    }
}