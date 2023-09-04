namespace Aws.Ssm.ClientTool.EnvironmentVariables.Services;

public class MacEnvironmentVariablesProvider : DefaultEnvironmentVariablesProvider
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