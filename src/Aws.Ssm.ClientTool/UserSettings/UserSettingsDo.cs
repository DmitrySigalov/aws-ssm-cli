namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsDo
{
    public string EnvironmentVariablePrefix { get; set; } = "SSM";

    public char EnvironmentVariableDelimeter { get; set; } = '_';

    public HashSet<string> SsmPaths { get; set; } = new HashSet<string>();
}