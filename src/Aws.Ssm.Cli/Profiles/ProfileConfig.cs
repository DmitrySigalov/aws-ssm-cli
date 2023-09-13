using Newtonsoft.Json;

namespace Aws.Ssm.Cli.Profiles;

public class ProfileConfig
{
    public string EnvironmentVariablePrefix { get; set; } = "SSM_";

    public HashSet<string> SsmPaths { get; set; } = new();

    [JsonIgnore]
    public bool IsValid => 
        EnvironmentVariablePrefix != null && 
        SsmPaths?.Any() == true;

    public void CopyFrom(ProfileConfig source)
    {
        if (source == null)
        {
            return;
        }
        
        EnvironmentVariablePrefix = source.EnvironmentVariablePrefix;

        SsmPaths = new HashSet<string>(source.SsmPaths ?? new HashSet<string>());
    }

    public ProfileConfig Clone() => (ProfileConfig) this.MemberwiseClone();
}