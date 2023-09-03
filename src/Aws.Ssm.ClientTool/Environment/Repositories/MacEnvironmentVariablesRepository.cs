namespace Aws.Ssm.ClientTool.Environment.Repositories;

public class MacEnvironmentVariablesRepository : DefaultEnvironmentVariablesRepository
{
    public override void Set(string name, string value)
    {
        System.Environment.SetEnvironmentVariable(name, value);
        // TODO
    }

    public override void Delete(string name)
    {
        System.Environment.SetEnvironmentVariable(name, null);
        // TODO
    }
}