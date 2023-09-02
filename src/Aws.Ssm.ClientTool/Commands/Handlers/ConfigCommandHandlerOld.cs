using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.UserSettings;
using Aws.Ssm.ClientTool.Utils;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ConfigCommandHandlerOld
{
    private readonly char[] _validEnvVarNameDelimeters = new[] { '_', '-', };

    private readonly UserSettingsRepository _userSettingsRepository;

    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;

    private readonly ISsmParametersRepository _ssmParametersRepository;
    
    public ConfigCommandHandlerOld(
        UserSettingsRepository userSettingsRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository,
        ISsmParametersRepository ssmParametersRepository)
    {
        _userSettingsRepository = userSettingsRepository;
        
        _environmentVariablesRepository = environmentVariablesRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "config";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        var oldUserSettings = SpinnerUtils.Run(
            _userSettingsRepository.Get,
            "Get user settings");

        var newUserSettings = oldUserSettings.Clone();

        const string ADD_NEW_PLACEHOLDER = "[Add New]";
        var ssmPathsToAddOrDelete = Prompt
            .MultiSelect(
                $"- Select {nameof(oldUserSettings.SsmPaths)} to add or delete",
                new [] { ADD_NEW_PLACEHOLDER }.Union(oldUserSettings.SsmPaths),
                minimum: 0)
            .OrderBy(x => x)
            .ToArray();
        
        if (ssmPathsToAddOrDelete.Contains(ADD_NEW_PLACEHOLDER))
        {
            var newSsmPath = Prompt.Input<string>(
                "Enter new ssm path (start from the /)",
                validators: new List<Func<object, ValidationResult>>
                {
                    (check) => ValidateSsmPath((string) check, oldUserSettings.SsmPaths),
                }).Trim();

            newUserSettings.SsmPaths.Add(newSsmPath);
        }

        var ssmPathsToDelete = ssmPathsToAddOrDelete
            .Where(x => x != ADD_NEW_PLACEHOLDER)
            .ToArray();
        if (ssmPathsToDelete.Any())
        {
            newUserSettings.SsmPaths.ExceptWith(ssmPathsToDelete);
        }

        newUserSettings.EnvironmentVariablePrefix = Prompt.Input<string>(
            $"Set {nameof(oldUserSettings.EnvironmentVariablePrefix)} (space is undefined)",
            defaultValue: oldUserSettings.EnvironmentVariablePrefix ?? " ",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) => ValidateEnvironmentVariablePrefix((string) check),
            }).Trim();

        newUserSettings.EnvironmentVariableDelimeter = Prompt.Select(
            $"Set {nameof(oldUserSettings.EnvironmentVariableDelimeter)}",
            items: _validEnvVarNameDelimeters,
            defaultValue: oldUserSettings.EnvironmentVariableDelimeter);

        newUserSettings.EnvironmentVariableNamingType = Prompt.Select(
            $"Set {nameof(oldUserSettings.EnvironmentVariableNamingType)}",
            new[] { UserSettingsDo.NamingTypeEnum.None, UserSettingsDo.NamingTypeEnum.UpperCase, UserSettingsDo.NamingTypeEnum.LowerCase, },
            defaultValue: oldUserSettings.EnvironmentVariableNamingType);

        if (oldUserSettings.SsmPaths.Any())
        {
            SpinnerUtils.Run(
                () =>
                {
                    var convertedEnvironmentVariableNames = oldUserSettings.SsmPaths
                        .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, oldUserSettings))
                        .ToArray();
                    
                    _environmentVariablesRepository.DeleteEnvironmentVariables(convertedEnvironmentVariableNames);
                },
                "Delete old environment variables");
        }

        SpinnerUtils.Run(
            () => _userSettingsRepository.Save(oldUserSettings),
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

        if (check.Contains(' ') ||
            check.Contains('/') ||
            check.Contains('$') ||
            check.Contains('@'))
        {
            return new ValidationResult("Invalid value - Contains invalid character");
        }

        return ValidationResult.Success;
    }

    private ValidationResult ValidateSsmPath(string check, IEnumerable<string> configuredSsmPaths)
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

        var firstFoundParameter = configuredSsmPaths.FirstOrDefault(x => check.StartsWith(x));
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