using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;

namespace Aws.Ssm.ClientTool.EnvironmentVariables.Rules;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSsmPath(
        string ssmPath,
        ProfileConfig profileSettings)
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

        result = result.Replace(SsmParametersConsts.KeyDelimeter, EnvironmentVariablesConsts.VariableNameDelimeter);

        foreach (var invalidChar in EnvironmentVariablesConsts.InvalidVariableNameCharacters)
        {
            result = result.Replace(invalidChar, EnvironmentVariablesConsts.VariableNameDelimeter);
        }

        result = result.ToUpper();
        
        return result;
    }
}