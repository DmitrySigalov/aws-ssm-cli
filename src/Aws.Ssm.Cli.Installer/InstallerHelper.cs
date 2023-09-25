using Microsoft.Extensions.Configuration;

namespace Aws.Ssm.Cli.Installer;

public static class InstallerHelper
{
    public static string GetBuildWorkingDirectory(IConfiguration configuration)
    {
        return "../../../../../";
    }
    
    public static string GetBuildOutputDirectory(IConfiguration configuration)
    {
        return "app";
    }
}