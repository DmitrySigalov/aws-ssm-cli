using Microsoft.Extensions.Configuration;

namespace Aws.Ssm.ClientTool.SsmParameters;

public class SsmParametersRepository
{
    private readonly IConfiguration _configuration;
    
    public SsmParametersRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public IDictionary<string, string> GetDictionaryBy(ISet<string> paths)
    {
        var configurationSection = 
            _configuration.GetSection(SsmParameterProcessor.SectionName)
            ?? throw new ArgumentException("Not loaded SSM parameters");

        var allParameters = new Dictionary<string, string>();
        configurationSection.Bind(allParameters);
        // var allParameters = 
        //     configurationSection
        //         .GetChildren()
        //         .ToDictionary(
        //             x => x.Key,
        //             y => y.Value);

        var result = new Dictionary<string, string>();

        foreach (var path in paths.OrderBy(x => x))
        {
            foreach (var keyItem in allParameters
                         .Where(x => ConvertToSsmFormat(x.Key).StartsWith(path))
                         .OrderBy(x => x.Key))
            {
                result.Add(
                    ConvertToSsmFormat(keyItem.Key),
                    keyItem.Value);                
            }
        }

        return result;
    }

    private static string ConvertToSsmFormat(string parameterName) => "/" + parameterName.Replace(":", "/");
}