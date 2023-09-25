using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Aws.Ssm.Cli.Installer;

public static class InstallerHelper
{
    private static string ClientToolName => "aws-ssm-cli";
    
    public static string GetBuildWorkingDirectory(IConfiguration configuration)
    {
        var path = configuration["buildWorkingDirectory"];
        if (!string.IsNullOrEmpty(path))
        {
            return path;
        }
        
        return "../";
    }
    
    public static string GetAppHomeDirectory(IConfiguration configuration)
    {
        var path = configuration["appHomePath"];
        if (!string.IsNullOrEmpty(path))
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            path = Path.Combine(
                GetBuildWorkingDirectory(configuration),
                path);

            path = Path.GetFullPath(path);

            return path;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var windowsPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            return Path.Combine(windowsPath, ClientToolName);
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return $"/usr/local/share/{ClientToolName}";
        }

        throw new NotSupportedException($"Not supported {RuntimeInformation.RuntimeIdentifier}");
    }
}