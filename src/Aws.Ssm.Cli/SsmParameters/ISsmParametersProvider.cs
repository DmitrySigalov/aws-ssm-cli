namespace Aws.Ssm.Cli.SsmParameters;

public interface ISsmParametersProvider
{
    IDictionary<string, string> GetDictionaryBy(ISet<string> paths);
}