namespace Aws.Ssm.ClientTool.Environment.Repositories;

public class DefaultEnvironmentVariablesRepository : IEnvironmentVariablesRepository
{
    public ISet<string> GetNames(string baseName = null)
    {
        baseName = baseName?.Trim();
        
        var result = System.Environment
            .GetEnvironmentVariables(EnvironmentConsts.EnvironmentVariableTarget)
            .Keys
            .Cast<string>()
            .ToHashSet();

        if (!string.IsNullOrEmpty(baseName))
        {
            result.ExceptWith(result.Where(x => x.StartsWith(baseName, StringComparison.InvariantCulture)));
        }

        return result;
    }

    public string Get(string name)
    {
        return System.Environment.GetEnvironmentVariable(name, EnvironmentConsts.EnvironmentVariableTarget);
    }

    public void Set(string name, string value)
    {
        System.Environment.SetEnvironmentVariable(name, value, EnvironmentConsts.EnvironmentVariableTarget);
    }

    public void Delete(string name)
    {
        System.Environment.SetEnvironmentVariable(name, null, EnvironmentConsts.EnvironmentVariableTarget);
    }
}