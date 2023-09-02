using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Utils;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class RunCommandHandler : ICommandHandler
{
    private readonly IProfilesRepository _profilesRepository;

    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;

    public RunCommandHandler(
        IProfilesRepository profilesRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentVariablesRepository = environmentVariablesRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "run";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleUtils.WriteLineNotification($"Process [{Name}] command");
        Console.WriteLine();

        var profileNames = SpinnerUtils.Run(
            _profilesRepository.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleUtils.WriteLineError("Not configured any profile");

            return Task.CompletedTask;
        }

        var lastActiveProfileName = _profilesRepository.ActiveName;
        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleUtils.WriteLineNotification($"Current active profile is [{lastActiveProfileName}]");
        }

        var selectedProfileName = Prompt.Select(
            "Select profile for the activation",
            items: profileNames,
            defaultValue: lastActiveProfileName);
        
        var selectedProfileDo = SpinnerUtils.Run(
            () => _profilesRepository.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        if (selectedProfileDo?.SsmPaths?.Any() != true)
        {
            ConsoleUtils.WriteLineError($"Not configured profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleUtils.WriteLineNotification($"Deactivate profile [{lastActiveProfileName}]");

            var lastActiveProfileDo = 
                lastActiveProfileName == selectedProfileName
                ? selectedProfileDo
                : SpinnerUtils.Run(
                    () => _profilesRepository.GetByName(lastActiveProfileName),
                    $"Read profile [{lastActiveProfileName}]");

            if (lastActiveProfileDo != null)
            {
                var deletedEnvironmentVariables = SpinnerUtils.Run(
                    () => _environmentVariablesRepository.DeleteAll(lastActiveProfileDo),
                    "Delete environment variables");
                
                deletedEnvironmentVariables.PrintEnvironmentVariables(lastActiveProfileDo);
            }
            else 
            {
                ConsoleUtils.WriteLineError($"Not configured profile [{lastActiveProfileDo}]");
            }
        }

        _profilesRepository.ActiveName = selectedProfileName;
        ConsoleUtils.WriteLineNotification($"Activate profile [{selectedProfileName}]");
        
        var resolvedSsmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(selectedProfileDo);

        if (resolvedSsmParameters.Any() == false)
        {
            ConsoleUtils.WriteLineError("NOT DONE - Unavailable ssm parameters");

            return Task.CompletedTask;
        }

        var appliedEnvironmentVariables = SpinnerUtils.Run(
            () => _environmentVariablesRepository.SetFromSsmParameters(
                resolvedSsmParameters,
                selectedProfileDo),
            $"Apply environment variables");
        
        appliedEnvironmentVariables.PrintEnvironmentVariables(
            resolvedSsmParameters,
            selectedProfileDo);
        
        ConsoleUtils.WriteLineInfo($"DONE - Activated profile [{selectedProfileName}]");

        return Task.CompletedTask;
    }
}