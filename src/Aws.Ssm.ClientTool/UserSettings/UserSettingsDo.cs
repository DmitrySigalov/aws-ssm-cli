namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsDo
{
    public string EnvVarNamePrefix { get; set; } = "SSM";

    public char EnvVarNameDelimeter { get; set; } = '_';

    public HashSet<string> SsmPaths { get; set; } = new HashSet<string>();
}