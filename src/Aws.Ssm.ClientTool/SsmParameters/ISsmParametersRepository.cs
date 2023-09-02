namespace Aws.Ssm.ClientTool.SsmParameters;

public interface ISsmParametersRepository
{
    IDictionary<string, string> GetDictionaryBy(ISet<string> paths);
}