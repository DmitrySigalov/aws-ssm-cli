using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleSystemsManagement.Model;

namespace Aws.Ssm.ClientTool.SsmParameters;

public class SsmParameterProcessor : DefaultParameterProcessor
{
    public static readonly string InternalConfigKeyDelimeter = Guid.NewGuid().ToString();
    
    public override string GetKey(Parameter parameter, string path)
    {
        return parameter
            .Name
            .Replace(KeyDelimiter, InternalConfigKeyDelimeter);;
    }
}