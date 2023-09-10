using System.Text;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.SsmParameters;

namespace Aws.Ssm.Cli.EnvironmentVariables.Rules;

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
            if (EnvironmentVariableNameValidationRule.InvalidVariableNameCharacters.Contains(c))
            {
                result.Append(EnvironmentVariablesConsts.VariableNameDelimeter);

                continue;
            }

            result.Append(c);
        }

        return result.ToString().ToUpper();
    }
}