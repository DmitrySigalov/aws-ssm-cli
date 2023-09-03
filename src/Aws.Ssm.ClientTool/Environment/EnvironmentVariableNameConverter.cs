using Aws.Ssm.ClientTool.Profiles;

namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentVariableNameConverter
{
    private static char SsmDelimeter => '/';
    private static char EnvDelimeter => '_';
    
    public static string ConvertFromSsmPath(
        string ssmPath,
        ProfileDo profileSettings)
    {
        var result = ssmPath;
        
        if (result.StartsWith(SsmDelimeter))
        {
            result = result.TrimStart(SsmDelimeter);
        }
        
        result = result.Replace(SsmDelimeter, EnvDelimeter);

        if (!string.IsNullOrEmpty(profileSettings.EnvironmentVariablePrefix))
        {
            result = profileSettings.EnvironmentVariablePrefix + result;
        }

        result = result.ToUpper();
        
        return result;
    }
}