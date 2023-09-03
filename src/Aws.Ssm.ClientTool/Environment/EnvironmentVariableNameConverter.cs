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
        
        result = result.Replace(SsmParametersConsts.KeyDelimeter, EnvironmentConsts.VariableNameDelimeter);

        if (!string.IsNullOrEmpty(profileSettings.EnvironmentVariablePrefix))
        {
            result = profileSettings.EnvironmentVariablePrefix + result;
        }

        result = result.ToUpper();
        
        return result;
    }
}