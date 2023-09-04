using System.ComponentModel.DataAnnotations;
using Aws.Ssm.ClientTool.EnvironmentVariables;
using Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;
using Aws.Ssm.ClientTool.EnvironmentVariables.Rules;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.Profiles.Extensions;
using Aws.Ssm.ClientTool.Profiles.Rules;
using Aws.Ssm.ClientTool.Validation;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ConfigCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;
    
    private enum OperationEnum
    {
        Create,
        Delete,
        Update,
    }

    public ConfigCommandHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profileConfigProvider = profileConfigProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _ssmParametersRepository = ssmParametersRepository;
    }

    public string Name => "config";

    public string Description => "Profile(s) configuration";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification(Description);
        Console.WriteLine();

        var profileDetails = GetProfileDetailsForConfiguration();

        if (profileDetails.Operation != OperationEnum.Create &&
            profileDetails.ProfileName == _profileConfigProvider.ActiveName &&
            profileDetails.ProfileDo.SsmPaths.Any() == true)
        {
            ConsoleHelper.WriteLineNotification($"Deactivate profile [{profileDetails.ProfileName}] before any configuration changes");

            var deletedEnvironmentVariables = SpinnerHelper.Run(
                () => _environmentVariablesProvider.DeleteAll(profileDetails.ProfileDo),
                "Delete environment variables");
                
            deletedEnvironmentVariables.PrintEnvironmentVariablesWithProfileValidation(profileDetails.ProfileDo);

            _profileConfigProvider.ActiveName = null;
        }

        if (profileDetails.Operation == OperationEnum.Create)
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

        var allowToExit = false;

        while (!allowToExit)
        {
            var exitOperationName = "Exit and view configured profile"; 
            var removeSsmPathOperationName = $"Remove from {nameof(profileDetails.ProfileDo.SsmPaths)}";
            var manageOperationsLookup = new Dictionary<string, Func<ProfileConfig, bool>>
            {
                { exitOperationName, Exit },
                { $"Add into {nameof(profileDetails.ProfileDo.SsmPaths)}", AddSsmPath },
                { removeSsmPathOperationName, RemoveSsmPaths },
                { $"Set {nameof(profileDetails.ProfileDo.EnvironmentVariablePrefix)}", SetEnvironmentVariablePrefix },
           };
            if (profileDetails.ProfileDo.SsmPaths.Any() != true)
            {
                manageOperationsLookup.Remove(exitOperationName);
                manageOperationsLookup.Remove(removeSsmPathOperationName);
            }

            var operationKey = Prompt.Select(
                "Select operation",
                items: manageOperationsLookup.Keys,
                defaultValue: manageOperationsLookup.Keys.First());

            var operationFunction = manageOperationsLookup[operationKey];

            allowToExit = operationFunction(profileDetails.ProfileDo);

            if (!allowToExit)
            {
                SpinnerHelper.Run(
                    () => _profileConfigProvider.Save(profileDetails.ProfileName, profileDetails.ProfileDo),
                    $"Save profile [{profileDetails.ProfileName}] configuration new settings");
            
                profileDetails.ProfileDo.PrintProfileSettings();
            }
        }

        ConsoleHelper.WriteLineInfo($"DONE - Configured profile [{profileDetails.ProfileName}]");
        Console.WriteLine();

        ConsoleHelper.WriteLineNotification($"View profile [{profileDetails.ProfileName}] configuration");
        Console.WriteLine();

        var resolvedSsmParameters = SpinnerHelper.Run(
            () => _ssmParametersRepository.GetDictionaryBy(profileDetails.ProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(profileDetails.ProfileDo);

        var actualEnvironmentVariables = SpinnerHelper.Run(
            () => _environmentVariablesProvider.GetAll(profileDetails.ProfileDo),
            "Get environment variables");

        actualEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidation(
            resolvedSsmParameters,
            profileDetails.ProfileDo);

        ConsoleHelper.WriteLineInfo($"DONE - Viewed profile [{profileDetails.ProfileName}]");

        return Task.CompletedTask;
    }

    private (OperationEnum Operation, string ProfileName, ProfileConfig ProfileDo) GetProfileDetailsForConfiguration()
    {
        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        var lastActiveProfileName = _profileConfigProvider.ActiveName;
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleHelper.WriteLineNotification($"Current active profile is [{lastActiveProfileName}]");
        }

        var operation = OperationEnum.Create;
        var profileName = "default";
        var profileDo = new ProfileConfig();

        if (profileNames.Any())
        {
            operation = Prompt.Select(
                "Select profile operation",
                items: new[] { OperationEnum.Update, OperationEnum.Create, OperationEnum.Delete },
                defaultValue: OperationEnum.Update);
        }

        if (operation == OperationEnum.Create)
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

        profileDo = 
            SpinnerHelper.Run(
                () => _profileConfigProvider.GetByName(profileName),
                $"Read profile [{profileName}]")
            ?? new ProfileConfig(); 

        return (operation, profileName, profileDo);
    }

    private bool Exit(ProfileConfig profileConfig) => true;
    
    private bool AddSsmPath(ProfileConfig profileConfig)
    {
        var newSsmPath = Prompt.Input<string>(
            "Enter new ssm-path (start from the /)",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) =>
                {
                    if (check == null) return ValidationResult.Success;
                    
                    return SsmPathValidationRules.Handle(
                        (string)check,
                        profileConfig.SsmPaths,
                        _ssmParametersRepository);
                },
            })?.Trim();

        if (!string.IsNullOrWhiteSpace(newSsmPath))
        {
            profileConfig.SsmPaths = new HashSet<string>(
                profileConfig.SsmPaths
                    .Union(new [] { newSsmPath })
                    .OrderBy(x => x));
        }

        return false;
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

        return false;
    }
    
    private bool SetEnvironmentVariablePrefix(ProfileConfig profileConfig)
    {
        profileConfig.EnvironmentVariablePrefix = Prompt.Input<string>(
            $"Set {nameof(profileConfig.EnvironmentVariablePrefix)} (space is undefined)",
            defaultValue: profileConfig.EnvironmentVariablePrefix ?? " ",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) => EnvironmentVariableNameValidationRule.HandlePrefix((string) check),
            }).Trim();

        return false;
    }
}