namespace Aws.Ssm.ClientTool.SsmParameters;

public interface ISsmParametersProvider
{
    IDictionary<string, string> GetDictionaryBy(ISet<string> paths);
}