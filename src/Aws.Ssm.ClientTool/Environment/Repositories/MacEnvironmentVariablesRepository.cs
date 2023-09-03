using Aws.Ssm.ClientTool.Helpers;

namespace Aws.Ssm.ClientTool.Environment.Repositories;

public class MacEnvironmentVariablesRepository : DefaultEnvironmentVariablesRepository
{
    public override void Set(string name, string value)
    {
        ConsoleHelper.WriteLineError($"{nameof(MacEnvironmentVariablesRepository)}.{nameof(Set)}({name}) not implemented");
    }

    public override void Delete(string name)
    {
        ConsoleHelper.WriteLineError($"{nameof(MacEnvironmentVariablesRepository)}.{nameof(Delete)}({name}) not implemented");
    }
}