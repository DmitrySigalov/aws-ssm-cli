namespace Aws.Ssm.ClientTool.Helpers;

public static class OperationSystemHelper
{
    public static bool IsMacPlatform(OperatingSystem operatingSystem = null)
    {
        operatingSystem ??= System.Environment.OSVersion;
        
        if (operatingSystem.Platform == PlatformID.MacOSX)
        {
            return true;
        }
        
        if (operatingSystem.Platform == PlatformID.Unix)
        {
            // Well, there are chances MacOSX is reported as Unix instead of MacOSX.
            // Instead of platform check, we'll do a feature checks (Mac specific root folders)
            return Directory.Exists("/Applications") &
                   Directory.Exists("/System") &
                   Directory.Exists("/Users") &
                   Directory.Exists("/Volumes");
        }

        return false;
    }
}