namespace Ssm.Aws.ClientTool.UserSettings;

public class UserSettingsDo
{
    public string Prefix { get; set; } = "SSM";

    public string Delimeter { get; set; } = "_";

    public HashSet<string> Paths { get; set; } = new HashSet<string>();
}