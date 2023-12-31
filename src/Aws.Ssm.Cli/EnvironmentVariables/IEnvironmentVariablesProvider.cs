namespace Aws.Ssm.Cli.EnvironmentVariables;

public interface IEnvironmentVariablesProvider
{
    ISet<string> GetNames(string baseName = null);

    string Get(string name);

    void Set(string name, string value);

    string CompleteActivationEnvironmentVariables();
}