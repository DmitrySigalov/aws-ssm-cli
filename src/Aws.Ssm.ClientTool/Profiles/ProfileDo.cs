namespace Aws.Ssm.ClientTool.Profiles;

public class ProfileDo
{
    public HashSet<string> SsmPaths { get; set; } = new();

    public string EnvironmentVariablePrefix { get; set; } = "SSM_";

    public ProfileDo Clone() => (ProfileDo) this.MemberwiseClone();
}