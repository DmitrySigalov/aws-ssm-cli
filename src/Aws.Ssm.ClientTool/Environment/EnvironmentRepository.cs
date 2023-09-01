namespace Aws.Ssm.ClientTool.Environment;

public class EnvironmentRepository
{
    public string GetEnvironmentVariable(string name)
    {
        return null;
    }

    public void DeleteEnvironmentVariable(string name)
    {
    }

    public ISet<string> GetEnvironmentVariableNames(IEnumerable<string> baseNames)
    {
        return baseNames.ToHashSet();
    }

    // TODO: delete
    public void DeleteEnvironmentVariables(IEnumerable<string> baseNames)
    {
    }
}