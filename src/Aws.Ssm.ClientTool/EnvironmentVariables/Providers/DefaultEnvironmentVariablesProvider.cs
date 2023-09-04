namespace Aws.Ssm.ClientTool.EnvironmentVariables.Providers;

public class DefaultEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    public ISet<string> GetNames(string baseName = null)
    {
        baseName = baseName?.Trim();
        
        var result = System.Environment
            .GetEnvironmentVariables()
            .Keys
            .Cast<string>()
            .ToHashSet();

        if (!string.IsNullOrEmpty(baseName))
        {
            result.ExceptWith(result.Where(x => !x.StartsWith(baseName, StringComparison.InvariantCulture)));
        }

        return result;
    }

    public string Get(string name)
    {
        return System.Environment.GetEnvironmentVariable(name);
    }

    public virtual void Set(string name, string value)
    {
        System.Environment.SetEnvironmentVariable(name, value, EnvironmentVariablesConsts.EnvironmentVariableTarget);
    }

    public virtual void Delete(string name)
    {
        System.Environment.SetEnvironmentVariable(name, null, EnvironmentVariablesConsts.EnvironmentVariableTarget);
    }
}