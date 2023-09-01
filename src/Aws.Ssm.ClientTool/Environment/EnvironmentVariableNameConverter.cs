using Aws.Ssm.ClientTool.UserSettings;

namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSsmPath(
        string ssmPath,
        UserSettingsDo userSettings)
    {
        var result = ssmPath;
        
        if (result.StartsWith('/'))
        {
            result = result.TrimStart('/');
        }
        
        result = result.Replace('/', userSettings.EnvironmentVariableDelimeter);

        if (!string.IsNullOrEmpty(userSettings.EnvironmentVariablePrefix))
        {
            result = userSettings.EnvironmentVariablePrefix + userSettings.EnvironmentVariableDelimeter + result;
        }
        
        return result;
    }
}