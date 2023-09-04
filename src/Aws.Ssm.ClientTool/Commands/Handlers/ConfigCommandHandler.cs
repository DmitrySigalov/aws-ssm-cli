using System.ComponentModel.DataAnnotations;
using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.Validation;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ConfigCommandHandler : ICommandHandler
{
    private readonly IProfilesRepository _profilesRepository;

    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;
    
    private enum OperationEnum
    {
        Create,
        Delete,
        Update,
    }

    public ConfigCommandHandler(
        IProfilesRepository profilesRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentVariablesRepository = environmentVariablesRepository;

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
            profileDetails.ProfileName == _profilesRepository.ActiveName &&
            profileDetails.ProfileDo.SsmPaths.Any() == true)
        {
            ConsoleHelper.WriteLineNotification($"Deactivate profile [{profileDetails.ProfileName}] before any configuration changes");

            var deletedEnvironmentVariables = SpinnerHelper.Run(
                () => _environmentVariablesRepository.DeleteAll(profileDetails.ProfileDo),
                "Delete environment variables");
                
            deletedEnvironmentVariables.PrintEnvironmentVariablesWithProfileValidation(profileDetails.ProfileDo);

            _profilesRepository.ActiveName = null;
        }

        if (profileDetails.Operation == OperationEnum.Create)
        {
            SpinnerHelper.Run(
                () => _profilesRepository.Save(profileDetails.ProfileName, profileDetails.ProfileDo),
                $"Save new profile [{profileDetails.ProfileName}] configuration with default settings");
        }

        profileDetails.ProfileDo.PrintProfileSettings();

        if (profileDetails.Operation == OperationEnum.Delete)
        {
            SpinnerHelper.Run(
                () => _profilesRepository.Delete(profileDetails.ProfileName),
                $"Delete profile [{profileDetails.ProfileName}]");
            
            ConsoleHelper.WriteLineInfo($"DONE - Deleted profile [{profileDetails.ProfileName}]");

            return Task.CompletedTask;
        }

        var allowToExit = false;

        while (!allowToExit)
        {
            var exitOperationName = "Exit and view configured profile"; 
            var removeSsmPathOperationName = $"Remove from {nameof(profileDetails.ProfileDo.SsmPaths)}";
            var manageOperationsLookup = new Dictionary<string, Func<ProfileDo, bool>>
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
                    () => _profilesRepository.Save(profileDetails.ProfileName, profileDetails.ProfileDo),
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
            () => _environmentVariablesRepository.GetAll(profileDetails.ProfileDo),
            "Get environment variables");

        actualEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidation(
            resolvedSsmParameters,
            profileDetails.ProfileDo);

        ConsoleHelper.WriteLineInfo($"DONE - Viewed profile [{profileDetails.ProfileName}]");

        return Task.CompletedTask;
    }

    private (OperationEnum Operation, string ProfileName, ProfileDo ProfileDo) GetProfileDetailsForConfiguration()
    {
        var profileNames = SpinnerHelper.Run(
            _profilesRepository.GetNames,
            "Get profile names");

        var lastActiveProfileName = _profilesRepository.ActiveName;
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleHelper.WriteLineNotification($"Current active profile is [{lastActiveProfileName}]");
        }

        var operation = OperationEnum.Create;
        var profileName = "default";
        var profileDo = new ProfileDo();

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
                    (check) => ProfileNameValidationRules.Handle((string) check, profileNames),
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
                () => _profilesRepository.GetByName(profileName),
                $"Read profile [{profileName}]")
            ?? new ProfileDo(); 

        return (operation, profileName, profileDo);
    }

    private bool Exit(ProfileDo profileDo) => true;
    
    private bool AddSsmPath(ProfileDo profileDo)
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
                        profileDo.SsmPaths,
                        _ssmParametersRepository);
                },
            })?.Trim();

        if (!string.IsNullOrWhiteSpace(newSsmPath))
        {
            profileDo.SsmPaths = new HashSet<string>(
                profileDo.SsmPaths
                    .Union(new [] { newSsmPath })
                    .OrderBy(x => x));
        }

        return false;
    }
    
    private bool RemoveSsmPaths(ProfileDo profileDo)
    {
        var ssmPathsToDelete = Prompt
            .MultiSelect(
                "- Select ssm-path(s) to delete",
                profileDo.SsmPaths,
                minimum: 0)
            .OrderBy(x => x)
            .ToArray();
        
        profileDo.SsmPaths.ExceptWith(ssmPathsToDelete);

        return false;
    }
    
    private bool SetEnvironmentVariablePrefix(ProfileDo profileDo)
    {
        profileDo.EnvironmentVariablePrefix = Prompt.Input<string>(
            $"Set {nameof(profileDo.EnvironmentVariablePrefix)} (space is undefined)",
            defaultValue: profileDo.EnvironmentVariablePrefix ?? " ",
            validators: new List<Func<object, ValidationResult>>
            {
                (check) => EnvironmentVariableNameValidationRules.HandlePrefix((string) check),
            }).Trim();

        return false;
    }
}