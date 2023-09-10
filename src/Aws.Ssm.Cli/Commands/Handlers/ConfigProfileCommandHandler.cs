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

        var profileDetails = GetProfileDetailsForConfiguration();

        if (profileDetails.Operation != OperationEnum.New &&
            profileDetails.ProfileName == _profileConfigProvider.ActiveName &&
            profileDetails.ProfileDo.IsValid)
        {
            ConsoleHelper.WriteLineNotification($"Deactivate profile [{profileDetails.ProfileName}] before any configuration changes");

            SpinnerHelper.Run(
                () => _environmentVariablesProvider.DeleteAll(profileDetails.ProfileDo),
                "Delete active environment variables");

            _profileConfigProvider.ActiveName = null;
        }

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

        var allowToExit = false;

        while (!allowToExit)
        {
            var exitOperationName = "Exit"; 
            var removeSsmPathOperationName = $"Remove from {nameof(profileDetails.ProfileDo.SsmPaths)}";
            var manageOperationsLookup = new Dictionary<string, Func<ProfileConfig, bool>>
            {
                { exitOperationName, Exit },
                { $"Add into {nameof(profileDetails.ProfileDo.SsmPaths)}", AddSsmPath },
                { removeSsmPathOperationName, RemoveSsmPaths },
                { $"Configure {nameof(profileDetails.ProfileDo.EnvironmentVariablePrefix)}", SetEnvironmentVariablePrefix },
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

        ConsoleHelper.WriteLineInfo($"DONE - Profile [{profileDetails.ProfileName}] configuration");
        Console.WriteLine();

        ConsoleHelper.WriteLineNotification($"START - View profile [{profileDetails.ProfileName}] configuration");
        Console.WriteLine();

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
                        _ssmParametersProvider);
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