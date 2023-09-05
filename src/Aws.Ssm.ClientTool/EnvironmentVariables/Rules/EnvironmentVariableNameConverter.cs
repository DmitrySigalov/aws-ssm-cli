using System.Text;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;

namespace Aws.Ssm.ClientTool.EnvironmentVariables.Rules;

public static class EnvironmentVariableNameConverter
{
    public static string ConvertFromSsmPath(
        string ssmPath,
        ProfileConfig profileSettings)
    {
        var result = new StringBuilder();

        result.Append(profileSettings.EnvironmentVariablePrefix);
        
        if (ssmPath.StartsWith(SsmParametersConsts.KeyDelimeter))
        {
            ssmPath = ssmPath.TrimStart(SsmParametersConsts.KeyDelimeter);
        }

        foreach (var c in ssmPath)
        {
            if (EnvironmentVariablesConsts.InvalidVariableNameCharacters.Contains(c))
            {
                result.Append(EnvironmentVariablesConsts.VariableNameDelimeter);

                continue;
            }

            result.Append(c);
        }

        return result.ToString().ToUpper();
    }
}