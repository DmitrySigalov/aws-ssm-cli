using Amazon.Extensions.Configuration.SystemsManager;
using Amazon.SimpleSystemsManagement.Model;

namespace Aws.Ssm.Cli.SsmParameters.Rules;

public class SsmParameterProcessor : DefaultParameterProcessor
{
    public override string GetKey(Parameter parameter, string path) => parameter.Name;
}