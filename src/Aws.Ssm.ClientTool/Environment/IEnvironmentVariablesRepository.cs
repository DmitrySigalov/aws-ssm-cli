namespace Aws.Ssm.ClientTool.Environment;

public interface IEnvironmentVariablesRepository
{
    ISet<string> GetNames(string baseName = null);

    string Get(string name);

    void Set(string name, string value);

    void Delete(string name);
}