using Aws.Ssm.Cli.GitHub;

namespace Aws.Ssm.Cli.VersionControl;

public class CheckVersionInfo
{
    public DateTime LastCheckTime { get; set; }
    
    public string LastCheckRuntimeReleaseVersion { get; set; }
    
    public GitHubModel.Release LatestRelease { get; set; }
}