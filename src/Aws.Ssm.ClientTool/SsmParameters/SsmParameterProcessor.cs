using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleSystemsManagement.Model;

namespace Aws.Ssm.ClientTool.SsmParameters;

public class SsmParameterProcessor : DefaultParameterProcessor
{
    public const string SectionName = "ssm";
    
    public override string GetKey(Parameter parameter, string path)
    {
        var resolvedKey = base.GetKey(parameter, path);
        
        return SectionName + KeyDelimiter + resolvedKey;
    }
}