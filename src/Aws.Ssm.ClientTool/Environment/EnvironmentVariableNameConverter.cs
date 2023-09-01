using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.UserSettings;

namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSsmPath(
        string ssmPath,
        ProfileDo profileSettings)
    {
        var result = ssmPath;
        
        if (result.StartsWith('/'))
        {
            result = result.TrimStart('/');
        }
        
        result = result.Replace('/', profileSettings.EnvironmentVariableDelimeter);

        if (!string.IsNullOrEmpty(profileSettings.EnvironmentVariablePrefix))
        {
            result = profileSettings.EnvironmentVariablePrefix + result;
        }

        if (profileSettings.EnvironmentVariableNamingConvertType == ProfileDo.NamingConvertTypeEnum.UpperCase)
        {
            result = result.ToUpper();
        }
        else if (profileSettings.EnvironmentVariableNamingConvertType == ProfileDo.NamingConvertTypeEnum.LowerCase)
        {
            result = result.ToLower();
        }
        
        return result;
    }

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
            result = userSettings.EnvironmentVariablePrefix + result;
        }

        if (userSettings.EnvironmentVariableNamingType == UserSettingsDo.NamingTypeEnum.UpperCase)
        {
            result = result.ToUpper();
        }
        else if (userSettings.EnvironmentVariableNamingType == UserSettingsDo.NamingTypeEnum.LowerCase)
        {
            result = result.ToLower();
        }
        
        return result;
    }
}