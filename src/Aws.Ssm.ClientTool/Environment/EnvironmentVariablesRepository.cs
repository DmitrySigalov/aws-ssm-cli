namespace Aws.Ssm.ClientTool.Environment;

public class EnvironmentVariablesRepository : IEnvironmentVariablesRepository
{
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

    public ISet<string> GetNames(IEnumerable<string> baseNames)
    {
        return baseNames.ToHashSet();
    }

    // TODO: delete
    public void DeleteEnvironmentVariables(IEnumerable<string> baseNames)
    {
    }
}