namespace Aws.Ssm.ClientTool.Environment.Repositories;

public class DummyEnvironmentVariablesRepository : IEnvironmentVariablesRepository
{
    public ISet<string> GetNames(string baseName = "")
    {
        return new HashSet<string>
        {
            baseName + "_*",
        };
    }

    public string Get(string name)
    {
        return "[VALUE]";
    }

    public void Set(string name, string value)
    {
    }

    public void Delete(string name)
    {
    }
}