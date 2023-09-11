using System.ComponentModel.DataAnnotations;

namespace Aws.Ssm.Cli.SsmParameters.Rules;

public static class SsmPathValidationRules
{
    public static ValidationResult Handle(
        string check,
        IEnumerable<string> configuredSsmPaths)
    {
        check = check?.Trim();

        configuredSsmPaths = configuredSsmPaths.ToArray();
        
        if (string.IsNullOrWhiteSpace(check))
        {
            return new ValidationResult("Invalid empty value");
        }
        
        if (!check.StartsWith(SsmParametersConsts.KeyDelimeter))
        {
            return new ValidationResult($"Invalid value - start from {SsmParametersConsts.KeyDelimeter}");
        }

        var firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => check.Equals(x, StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated ssm path - {firstFoundParameter} ");
        }

        firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => check.StartsWith(
            x.EndsWith(SsmParametersConsts.KeyDelimeter) ? x : x + SsmParametersConsts.KeyDelimeter, 
            StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated parent ssm path - {firstFoundParameter} ");
        }

        firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => x.StartsWith(
            check.EndsWith(SsmParametersConsts.KeyDelimeter) ? check : check + SsmParametersConsts.KeyDelimeter, 
            StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated child ssm path - {firstFoundParameter} ");
        }

        return ValidationResult.Success;
    }
}