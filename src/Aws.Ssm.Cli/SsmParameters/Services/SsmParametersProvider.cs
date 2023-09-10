using Aws.Ssm.Cli.SsmParameters.Rules;
using Microsoft.Extensions.Configuration;

namespace Aws.Ssm.Cli.SsmParameters.Services;

public class SsmParametersProvider : ISsmParametersProvider
{
    public IDictionary<string, string> GetDictionaryBy(ISet<string> paths)
    {
        if (paths.Any() == false)
        {
            return new Dictionary<string, string>();
        }

        var configuration = LoadSystemParameters(paths);
        
        var result = new SortedDictionary<string, string>();
        configuration.Bind(result);

        return result;
    }

    private IConfiguration LoadSystemParameters(IEnumerable<string> paths)
    {
        var configurationBuilder = new ConfigurationBuilder();

        foreach (var path in paths)
        {
            configurationBuilder.AddSystemsManager(configurationSource =>
            {
                configurationSource.Path = path;
                configurationSource.ParameterProcessor = new SsmParameterProcessor();
            });
        }

        return configurationBuilder.Build();
    }
}