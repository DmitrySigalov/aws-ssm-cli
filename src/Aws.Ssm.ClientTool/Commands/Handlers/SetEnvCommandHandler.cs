using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Helpers;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class SetEnvCommandHandler : ICommandHandler
{
    private readonly IProfilesRepository _profilesRepository;

    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;

    public SetEnvCommandHandler(
        IProfilesRepository profilesRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentVariablesRepository = environmentVariablesRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "set-env";
    
    public string Description => "Set environment variable(s) from profile configuration";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification(Description);
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profilesRepository.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("Not configured any profile");

            return Task.CompletedTask;
        }

        var lastActiveProfileName = _profilesRepository.ActiveName;
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleHelper.WriteLineNotification($"Current active profile is [{lastActiveProfileName}]");
        }

        var selectedProfileName = 
            profileNames.Count == 1
            ? profileNames.Single()
            : Prompt.Select(
                "Select profile for the activation",
                items: profileNames,
                defaultValue: lastActiveProfileName);
        
        var selectedProfileDo = SpinnerHelper.Run(
            () => _profilesRepository.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        selectedProfileDo?.PrintProfileSettings();

        if (selectedProfileDo?.SsmPaths?.Any() != true)
        {
            ConsoleHelper.WriteLineError($"Not configured profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleHelper.WriteLineNotification($"Deactivate profile [{lastActiveProfileName}] before new reactivation");

            var lastActiveProfileDo = 
                lastActiveProfileName == selectedProfileName
                ? selectedProfileDo
                : SpinnerHelper.Run(
                    () => _profilesRepository.GetByName(lastActiveProfileName),
                    $"Read profile [{lastActiveProfileName}]");

            if (lastActiveProfileDo != null)
            {
                var deletedEnvironmentVariables = SpinnerHelper.Run(
                    () => _environmentVariablesRepository.DeleteAll(lastActiveProfileDo),
                    "Delete environment variables");
                
                deletedEnvironmentVariables.PrintEnvironmentVariablesWithProfileValidation(lastActiveProfileDo);
            }
            else 
            {
                ConsoleHelper.WriteLineError($"Not configured profile [{lastActiveProfileName}]");
            }
        }

        _profilesRepository.ActiveName = selectedProfileName;
        ConsoleHelper.WriteLineNotification($"Activate profile [{selectedProfileName}]");
        
        var resolvedSsmParameters = SpinnerHelper.Run(
            () => _ssmParametersRepository.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(selectedProfileDo);

        if (resolvedSsmParameters.Any() == false)
        {
            ConsoleHelper.WriteLineError("NOT DONE - Unavailable ssm parameters");

            return Task.CompletedTask;
        }

        var appliedEnvironmentVariables = SpinnerHelper.Run(
            () => _environmentVariablesRepository.SetFromSsmParameters(
                resolvedSsmParameters,
                selectedProfileDo),
            $"Apply environment variables");
        
        appliedEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidation(
            resolvedSsmParameters,
            selectedProfileDo);
        
        ConsoleHelper.WriteLineInfo($"DONE - Activated profile [{selectedProfileName}]");

        return Task.CompletedTask;
    }
}