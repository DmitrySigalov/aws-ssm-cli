namespace Aws.Ssm.ClientTool.Environment;

public interface IEnvironmentVariablesRepository
{
    string Get(string name);

    void Set(string name, string value);

    void Delete(string name);

    ISet<string> GetNames(IEnumerable<string> baseNames);

    // TODO: delete
    void DeleteEnvironmentVariables(IEnumerable<string> baseNames);
}