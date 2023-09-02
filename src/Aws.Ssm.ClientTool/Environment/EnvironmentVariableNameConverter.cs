using Aws.Ssm.ClientTool.Profiles;

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
}