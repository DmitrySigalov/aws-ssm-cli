using System.ComponentModel.DataAnnotations;

namespace Aws.Ssm.ClientTool.EnvironmentVariables.Rules;

public static class EnvironmentVariableNameValidationRule
{
    public static ValidationResult HandlePrefix(string check)
    {
        check = check?.Trim();
        
        if (string.IsNullOrWhiteSpace(check))
        {
            return ValidationResult.Success; //Valid values
        }

        if (check.Length > 10)
        {
            return new ValidationResult("Invalid value - Too long value (exceeded 10 characters)");
        }

        if (check.Contains(' ') ||
            check.Contains('/') ||
            check.Contains('\\') ||
            check.Contains('|') ||
            check.Contains('"') ||
            check.Contains('\'') ||
            check.Contains('$') ||
            check.Contains('!') ||
            check.Contains('`') ||
            check.Contains('^') ||
            check.Contains('&') ||
            check.Contains('(') ||
            check.Contains(')') ||
            check.Contains('@'))
        {
            return new ValidationResult("Invalid value - Contains invalid character");
        }

        return ValidationResult.Success;
    }
}