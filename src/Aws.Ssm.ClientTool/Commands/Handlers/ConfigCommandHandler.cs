using System.ComponentModel.DataAnnotations;
using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.UserSettings;
using Aws.Ssm.ClientTool.Utils;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ConfigCommandHandler : ICommandHandler
{
    private readonly char[] _validEnvVarNameDelimeters = new[] { '.', ':', ';', '_', '-', };

    private readonly UserSettingsRepository _userSettingsRepository;

    private readonly EnvironmentRepository _environmentRepository;

    private readonly SsmParametersRepository _ssmParametersRepository;
    
    private enum OperationEnum
    {
        Add,
        Delete,
    }
    
    public ConfigCommandHandler(
        UserSettingsRepository userSettingsRepository,
        EnvironmentRepository environmentRepository,
        SsmParametersRepository ssmParametersRepository)
    {
        _userSettingsRepository = userSettingsRepository;
        
        _environmentRepository = environmentRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "config";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        var userSettings = SpinnerUtils.Run(
            _userSettingsRepository.Get,
            "Get user settings");

        userSettings.EnvironmentVariablePrefix = Prompt.Input<string>(
            $"Set {nameof(userSettings.EnvironmentVariablePrefix)} (space is undefined)",
            defaultValue: userSettings.EnvironmentVariablePrefix,
            validators: new List<Func<object, ValidationResult>>
            {
                (check) => ValidateEnvironmentVariablePrefix((string) check),
            }).Trim();

        userSettings.EnvironmentVariableDelimeter = Prompt.Select(
            $"Set {nameof(userSettings.EnvironmentVariableDelimeter)}",
            items: _validEnvVarNameDelimeters,
            defaultValue: userSettings.EnvironmentVariableDelimeter);

        var allowedOperations = new List<OperationEnum>();
        allowedOperations.AddRange(new [] { OperationEnum.Add });
        if (userSettings.SsmPaths.Any() == true)
        {
            allowedOperations.Add(OperationEnum.Delete);
        }
        var operation = Prompt.Select<OperationEnum>(
            $"Select operation to edit {nameof(userSettings.SsmPaths)}",
            defaultValue: OperationEnum.Add,
            items: allowedOperations);

        if (operation == OperationEnum.Delete)
        {
            var pathsToDelete = Prompt
                .MultiSelect(
                    $"- Select {nameof(userSettings.SsmPaths)} to delete",
                    userSettings.SsmPaths,
                    minimum: 0)
                .OrderBy(x => x)
                .ToArray();

            if (pathsToDelete.Any())
            {
                foreach (var path in pathsToDelete)
                {
                    userSettings.SsmPaths.Remove(path);
                }
            
                SpinnerUtils.Run(
                    () =>
                    {
                        var convertedEnvironmentVariableNames = pathsToDelete
                            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, userSettings))
                            .ToArray();
                    
                        _environmentRepository.DeleteEnvironmentVariables(convertedEnvironmentVariableNames);
                    },
                    "Delete environment variables");
            }
        }
        else if (operation == OperationEnum.Add)
        {
            var newSsmPath = Prompt.Input<string>(
                "Enter new ssm path (start from the /)",
                validators: new List<Func<object, ValidationResult>>
                {
                    (check) => ValidateSsmPath((string) check, userSettings),
                }).Trim();

            userSettings.SsmPaths.Add(newSsmPath);
        }

        SpinnerUtils.Run(
            () => _userSettingsRepository.Save(userSettings),
            "Save user settings");

        return Task.CompletedTask;
    }

    private ValidationResult ValidateEnvironmentVariablePrefix(string check)
    {
        if (check == " ")
        {
            return ValidationResult.Success; //Valid values
        }

        if (string.IsNullOrWhiteSpace(check))
        {
            return new ValidationResult("Empty value");
        }

        if (check.Contains(' '))
        {
            return new ValidationResult("Invalid value - Contains white space");
        }

        if (check.Contains('/') ||
            check.Contains('$') ||
            check.Contains('@'))
        {
            return new ValidationResult("Invalid value - Contains invalid character");
        }

        return ValidationResult.Success;
    }

    private ValidationResult ValidateSsmPath(string check, UserSettingsDo userSettings)
    {
        if (string.IsNullOrWhiteSpace(check) ||
            check.Replace("/", "") == "")
        {
            return new ValidationResult("Invalid value");
        }

        if (!check.StartsWith("/"))
        {
            return new ValidationResult("Invalid value - start from /");
        }

        var firstFoundParameter = userSettings.SsmPaths.FirstOrDefault(x => check.StartsWith(x));
        if (firstFoundParameter != null)
        {
            return new ValidationResult($"Duplicated value - {firstFoundParameter}");
        }

        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(new HashSet<string> { check, }),
            "Get ssm parameters from AWS System Manager to validate the parameters");

        if (ssmParameters?.Any() != true)
        {
            return new ValidationResult("Unavailable ssm path");
        }

        return ValidationResult.Success;
    }
}