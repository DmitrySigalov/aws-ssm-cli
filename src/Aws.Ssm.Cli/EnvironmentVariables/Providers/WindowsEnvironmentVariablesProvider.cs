namespace Aws.Ssm.Cli.EnvironmentVariables.Providers;

public class WindowsEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    public ISet<string> GetNames(string baseName = null)
    {
        baseName = baseName?.Trim();
        
        var result = Environment
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
        return Environment.GetEnvironmentVariable(name);
    }

    public void Set(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
    }
}