using System.ComponentModel.DataAnnotations;
using Aws.Ssm.Cli.EnvironmentVariables;
using Aws.Ssm.Cli.EnvironmentVariables.Rules;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.Profiles.Rules;
using Aws.Ssm.Cli.SsmParameters;
using Aws.Ssm.Cli.SsmParameters.Rules;
using Aws.Ssm.Cli.EnvironmentVariables.Extensions;
using Aws.Ssm.Cli.Profiles.Extensions;
using Aws.Ssm.Cli.SsmParameters.Extensions;
using Sharprompt;
using TextCopy;

namespace Aws.Ssm.Cli.Commands.Handlers;

public class ConfigProfileCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    
    private readonly ISsmParametersProvider _ssmParametersProvider;
    
    private enum OperationEnum
    {
        New,
        Delete,
        Edit,
    }

    public ConfigProfileCommandHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISsmParametersProvider ssmParametersProvider)
    {
        _profileConfigProvider = profileConfigProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _ssmParametersProvider = ssmParametersProvider;
    }

    public string CommandName => "config";

    public string Description => "Profile(s) configuration";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
        Console.WriteLine();
        
        var lastActiveProfileName = _profileConfigProvider.ActiveName;
        var lastActiveProfileDo = default(ProfileConfig);
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            _profileConfigProvider.ActiveName = null;

            lastActiveProfileDo = _profileConfigProvider.GetByName(lastActiveProfileName);

            if (lastActiveProfileDo?.IsValid == true)
            {
                ConsoleHelper.WriteLineNotification($"Deactivate current profile [{lastActiveProfileName}] before any configuration changes");

                SpinnerHelper.Run(
                    () => _environmentVariablesProvider.DeleteAll(lastActiveProfileDo),
                    "Delete active environment variables");
            }
        }

        var profileDetails = GetProfileDetailsForConfiguration(lastActiveProfileName, lastActiveProfileDo);

        if (profileDetails.Operation == OperationEnum.New)
        {
            SpinnerHelper.Run(
                () => _profileConfigProvider.Save(profileDetails.ProfileName, profileDetails.ProfileDo),
                $"Save new profile [{profileDetails.ProfileName}] configuration with default settings");
        }

        profileDetails.ProfileDo.PrintProfileSettings();

        if (profileDetails.Operation == OperationEnum.Delete)
        {
            SpinnerHelper.Run(
                () => _profileConfigProvider.Delete(profileDetails.ProfileName),
                $"Delete profile [{profileDetails.ProfileName}]");
            
            ConsoleHelper.WriteLineInfo($"DONE - Deleted profile [{profileDetails.ProfileName}]");

            return Task.CompletedTask;
        }

        string lastSelectedOperationKey = null;        

        var completeExitOperationKey = "Complete/exit configuration"; 

        while (lastSelectedOperationKey != completeExitOperationKey)
        {
            var removeSsmPathOperationKey = "Remove ssm-path(s)";
            var manageOperationsLookup = new Dictionary<string, Func<ProfileConfig, bool>>
            {
                { completeExitOperationKey, Exit },
                { "Set environment variable prefix", SetEnvironmentVariablePrefix },
                { "Add ssm-path (ignore availability)", (profile) => AddSsmPath(profile, allowAddUnavailableSsmPath: true) },
                { "Add ssm-path (available only)", (profile) => AddSsmPath(profile, allowAddUnavailableSsmPath: false) },
                { removeSsmPathOperationKey, RemoveSsmPaths },
                { "Import json configuration (enter)", ImportJsonConfiguration },
                { "Import json configuration (paste from clipboard)", ImportJsonConfigurationFromClipboard },
                { "Export json configuration (copy into clipboard)", ExportJsonConfigurationIntoClipboard },
           };
            if (profileDetails.ProfileDo.SsmPaths.Any() != true)
            {
                manageOperationsLookup.Remove(removeSsmPathOperationKey);
            }

            lastSelectedOperationKey = Prompt.Select(
                "Select operation",
                items: manageOperationsLookup.Keys,
                defaultValue: manageOperationsLookup.Keys.First());

            var operationFunction = manageOperationsLookup[lastSelectedOperationKey];

            var hasChanges = operationFunction(profileDetails.ProfileDo);

            if (hasChanges)
            {
                SpinnerHelper.Run(
                    () => _profileConfigProvider.Save(profileDetails.ProfileName, profileDetails.ProfileDo),
                    $"Save profile [{profileDetails.ProfileName}] configuration new settings");
            
                profileDetails.ProfileDo.PrintProfileSettings();
            }
        }

        ConsoleHelper.WriteLineInfo($"DONE - Profile [{profileDetails.ProfileName}] configuration");
        Console.WriteLine();

        ConsoleHelper.WriteLineNotification($"START - View profile [{profileDetails.ProfileName}] configuration");
        Console.WriteLine();

        if (profileDetails.ProfileDo.IsValid != true)
        {
            ConsoleHelper.WriteLineError($"Not configured profile [{profileDetails.ProfileName}]");

            return Task.CompletedTask;
        }

        var resolvedSsmParameters = SpinnerHelper.Run(
            () => _ssmParametersProvider.GetDictionaryBy(profileDetails.ProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(profileDetails.ProfileDo);

        resolvedSsmParameters.PrintSsmParameterToEnvironmentVariableNamesMapping(
            profileDetails.ProfileDo);

        resolvedSsmParameters.PrintSsmParametersToEnvironmentVariables(
            profileDetails.ProfileDo);

        ConsoleHelper.WriteLineInfo($"DONE - View profile [{profileDetails.ProfileName}] configuration");

        return Task.CompletedTask;
    }

    private (OperationEnum Operation, string ProfileName, ProfileConfig ProfileDo) GetProfileDetailsForConfiguration(
        string lastActiveProfileName,
        ProfileConfig currentProfileDo)
    {
        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        var operation = OperationEnum.New;
        var profileName = "default";
        var profileDo = new ProfileConfig();

        if (profileNames.Any())
        {
            operation = Prompt.Select(
                "Select profile operation",
                items: new[] { OperationEnum.Edit, OperationEnum.New, OperationEnum.Delete },
                defaultValue: OperationEnum.Edit);
        }

        if (operation == OperationEnum.New)
        {
            profileName = Prompt.Input<string>(
                "Enter new profile name ",
                defaultValue: profileName,
                validators: new List<Func<object, ValidationResult>>
                {
                    (check) => ProfileNameValidationRule.Handle((string) check, profileNames),
                }).Trim();
            
            return (operation, profileName, profileDo);
        }

        profileName =
            profileNames.Count == 1
                ? profileNames.Single()
                : Prompt.Select(
                    "Select profile",
                    items: profileNames,
                    defaultValue: lastActiveProfileName);

        profileDo = lastActiveProfileName == profileName
            ? currentProfileDo
            : SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(profileName),
                $"Read selected profile [{profileName}]");

        profileDo ??= new ProfileConfig(); 

        return (operation, profileName, profileDo);
    }

    private bool Exit(ProfileConfig profileConfig) => false;
    
    private bool SetEnvironmentVariablePrefix(ProfileConfig profileConfig)
    {
        var newPrefix = Prompt.Input<string>(
            $"Set {nameof(profileConfig.EnvironmentVariablePrefix)} (space is undefined)",
            defaultValue: profileConfig.EnvironmentVariablePrefix ?? " ",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) => EnvironmentVariableNameValidationRule.HandlePrefix((string) check),
            })?.Trim() ?? string.Empty;

        var hasChanges = profileConfig.EnvironmentVariablePrefix != newPrefix;

        if (hasChanges)
        {
            profileConfig.EnvironmentVariablePrefix = newPrefix;

            return true;
        }
        
        return false;
    }
    
    private bool AddSsmPath(ProfileConfig profileConfig, bool allowAddUnavailableSsmPath)
    {
        var newSsmPath = Prompt.Input<string>(
            "Enter new ssm-path (start from the /)",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) =>
                {
                    if (check == null)
                    {
                        return ValidationResult.Success;
                    }
                    
                    return SsmPathValidationRules.Handle(
                        (string) check,
                        profileConfig.SsmPaths);
                },
                (check) =>
                {
                    if (check == null)
                    {
                        return ValidationResult.Success;
                    }
                    
                    return CheckSsmPathAvailability(check.ToString(), allowAddUnavailableSsmPath);
                },
            })?.Trim();

        if (!string.IsNullOrWhiteSpace(newSsmPath))
        {
            profileConfig.SsmPaths = new HashSet<string>(
                profileConfig.SsmPaths
                    .Union(new [] { newSsmPath })
                    .OrderBy(x => x));

            return true;
        }

        return false;
    }

    private ValidationResult CheckSsmPathAvailability(string check, bool allowAddUnavailableSsmPath)
    {
        var ssmParameters = SpinnerHelper.Run(
            () => _ssmParametersProvider.GetDictionaryBy(new HashSet<string> { check, }),
            "Get ssm parameters from AWS System Manager to validate the ssm-path");

        if (ssmParameters?.Any() != true && !allowAddUnavailableSsmPath)
        {
            return new ValidationResult("Unavailable ssm path");
        }

        return ValidationResult.Success;
    }

    private bool RemoveSsmPaths(ProfileConfig profileConfig)
    {
        var ssmPathsToDelete = Prompt
            .MultiSelect(
                "- Select ssm-path(s) to delete",
                profileConfig.SsmPaths,
                minimum: 0)
            .OrderBy(x => x)
            .ToArray();

        profileConfig.SsmPaths.ExceptWith(ssmPathsToDelete);

        return ssmPathsToDelete.Length > 0;
    }

    private bool ImportJsonConfiguration(ProfileConfig profileConfig)
    {
        var newJson = Prompt.Input<string>(
            "Copy json",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) =>
                {
                    if (check == null)
                    {
                        return ValidationResult.Success;
                    }

                    var newProfileConfig = JsonSerializationHelper.Deserialize<ProfileConfig>(
                        (string) check);

                    if (newProfileConfig?.IsValid != true)
                    {
                        return new ValidationResult("Invalid profile configuration");
                    }

                    return ValidationResult.Success;
                },
            });

        if (!string.IsNullOrWhiteSpace(newJson))
        {
            var newProfileConfig = JsonSerializationHelper.Deserialize<ProfileConfig>(newJson);

            profileConfig.CopyFrom(newProfileConfig);

            return true;
        }

        return false;
    }

    private bool ImportJsonConfigurationFromClipboard(ProfileConfig profileConfig)
    {
        var newJson = ClipboardService.GetText()?.Trim(); 
        
        if (!string.IsNullOrWhiteSpace(newJson))
        {
            try
            {
                var newProfileConfig = JsonSerializationHelper.Deserialize<ProfileConfig>(newJson);

                if (newProfileConfig?.IsValid != true)
                {
                    throw new ApplicationException("Invalid profile configuration");
                }

                profileConfig.CopyFrom(newProfileConfig);

                return true;
            }
            catch (Exception e)
            {
                ConsoleHelper.WriteLineNotification(newJson);

                ConsoleHelper.WriteLineError(e.Message);

                return false;
            }
        }

        return false;
    }
    
    private bool ExportJsonConfigurationIntoClipboard(ProfileConfig profileConfig)
    {
        if (profileConfig.IsValid == false)
        {
            ConsoleHelper.WriteLineError("Invalid profile configuration");

            return false;
        }
        
        var json = JsonSerializationHelper.Serialize(profileConfig);
        
        ClipboardService.SetText(json);

        return false;
    }
}