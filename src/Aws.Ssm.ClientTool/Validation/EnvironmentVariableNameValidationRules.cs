using System.ComponentModel.DataAnnotations;

namespace Aws.Ssm.ClientTool.Validation;

public static class EnvironmentVariableNameValidationRules
{
    public static ValidationResult HandlePrefix(string check)
    {
        check = check?.Trim();
        
        if (string.IsNullOrWhiteSpace(check))
        {
            return ValidationResult.Success; //Valid values
        }

        if (check.Contains(' ') ||
            check.Contains('/') ||
            check.Contains('\\') ||
            check.Contains('|') ||
            check.Contains('"') ||
            check.Contains('\'') ||
            check.Contains('$') ||
            check.Contains('@'))
        {
            return new ValidationResult("Invalid value - Contains invalid character");
        }

        return ValidationResult.Success;
    }
}