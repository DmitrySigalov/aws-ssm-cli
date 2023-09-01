namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsDo
{
    public HashSet<string> SsmPaths { get; set; } = new HashSet<string>();

    public string EnvironmentVariablePrefix { get; set; }

    public char EnvironmentVariableDelimeter { get; set; } = '_';

    public enum NamingTypeEnum
    {
        None = 0,
        UpperCase = 1,
        LowerCase = 2,
    }
    
    public NamingTypeEnum EnvironmentVariableNamingType { get; set; } = NamingTypeEnum.UpperCase;

    public UserSettingsDo Clone() => (UserSettingsDo) this.MemberwiseClone();
}