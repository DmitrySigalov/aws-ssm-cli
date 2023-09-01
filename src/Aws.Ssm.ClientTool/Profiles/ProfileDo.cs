namespace Aws.Ssm.ClientTool.Profiles;

public class ProfileDo
{
    public HashSet<string> SsmPaths { get; set; } = new HashSet<string>();

    public string EnvironmentVariablePrefix { get; set; }

    public char EnvironmentVariableDelimeter { get; set; } = '_';

    public enum NamingConvertTypeEnum
    {
        UpperCase = 1,
        LowerCase = 2,
    }
    
    public NamingConvertTypeEnum EnvironmentVariableNamingConvertType { get; set; } = NamingConvertTypeEnum.UpperCase;

    public ProfileDo Clone() => (ProfileDo) this.MemberwiseClone();
}