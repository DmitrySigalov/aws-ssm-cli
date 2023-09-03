using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;

namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSsmPath(
        string ssmPath,
        ProfileDo profileSettings)
    {
        var result = ssmPath;
        
        if (result.StartsWith(SsmParametersConsts.KeyDelimeter))
        {
            result = result.TrimStart(SsmParametersConsts.KeyDelimeter);
        }
        
        if (!string.IsNullOrEmpty(profileSettings.EnvironmentVariablePrefix))
        {
            result = profileSettings.EnvironmentVariablePrefix + result;
        }

        result = result.Replace(SsmParametersConsts.KeyDelimeter, EnvironmentConsts.VariableNameDelimeter);

        foreach (var invalidChar in EnvironmentConsts.InvalidVariableNameCharacters)
        {
            result = result.Replace(invalidChar, EnvironmentConsts.VariableNameDelimeter);
        }

        result = result.ToUpper();
        
        return result;
    }
}