namespace Aws.Ssm.ClientTool.Profiles;

public class ProfileDo
{
    public HashSet<string> SsmPaths { get; set; } = new();

    public string EnvironmentVariablePrefix { get; set; } = "SSM_";

    public char EnvironmentVariableDelimeter { get; set; } = '_';

    public enum NamingConvertTypeEnum
    {
        None = 0,
        UpperCase = 1,
        LowerCase = 2,
    }
    
    public NamingConvertTypeEnum EnvironmentVariableNamingConvertType { get; set; } = NamingConvertTypeEnum.UpperCase;

    public ProfileDo Clone() => (ProfileDo) this.MemberwiseClone();
}