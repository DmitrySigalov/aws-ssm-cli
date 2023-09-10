using Newtonsoft.Json;

namespace Aws.Ssm.Cli.Profiles;

public class ProfileConfig
{
    public HashSet<string> SsmPaths { get; set; } = new();

    public string EnvironmentVariablePrefix { get; set; } = "SSM_";

    [JsonIgnore]
    public bool IsValid => SsmPaths?.Any() == true;

    public ProfileConfig Clone() => (ProfileConfig) this.MemberwiseClone();
}