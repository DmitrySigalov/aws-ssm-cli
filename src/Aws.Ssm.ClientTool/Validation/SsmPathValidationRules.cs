using System.ComponentModel.DataAnnotations;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Utils;

namespace Aws.Ssm.ClientTool.Validation;

public static class SsmPathValidationRules
{
    public static ValidationResult Handle(
        string check, 
        IEnumerable<string> configuredSsmPaths,
        ISsmParametersRepository ssmParametersRepository)
    {
        check = check?.Trim();

        configuredSsmPaths = configuredSsmPaths.ToArray();
        
        if (string.IsNullOrWhiteSpace(check))
        {
            return new ValidationResult("Invalid empty value");
        }
        
        if (!check.StartsWith("/"))
        {
            return new ValidationResult("Invalid value - start from /");
        }

        var firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => check.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated parent path - {firstFoundParameter} ");
        }

        firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => x.StartsWith(check, StringComparison.InvariantCultureIgnoreCase));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated child path - {firstFoundParameter} ");
        }

        var ssmParameters = SpinnerUtils.Run(
            () => ssmParametersRepository.GetDictionaryBy(new HashSet<string> { check, }),
            "Get ssm parameters from AWS System Manager to validate the ssm-path");

        if (ssmParameters?.Any() != true)
        {
            return new ValidationResult("Unavailable ssm path");
        }

        return ValidationResult.Success;
    }
}