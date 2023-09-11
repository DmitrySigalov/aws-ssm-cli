using System.ComponentModel.DataAnnotations;
using Aws.Ssm.Cli.Helpers;

namespace Aws.Ssm.Cli.SsmParameters.Rules;

public static class SsmPathValidationRules
{
    public static ValidationResult Handle(
        string check,
        ISsmParametersProvider ssmParametersProvider, 
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

        var firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => check.StartsWith(x + SsmParametersConsts.KeyDelimeter, StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated parent ssm path - {firstFoundParameter} ");
        }

        firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => x.StartsWith(check + SsmParametersConsts.KeyDelimeter, StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated child ssm path - {firstFoundParameter} ");
        }

        var ssmParameters = SpinnerHelper.Run(
            () => ssmParametersProvider.GetDictionaryBy(new HashSet<string> { check, }),
            "Get ssm parameters from AWS System Manager to validate the ssm-path");

        if (ssmParameters?.Any() != true)
        {
            return new ValidationResult("Unavailable ssm path");
        }

        return ValidationResult.Success;
    }
}