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
        var profileNames = SpinnerUtils.Run(
            _profilesRepository.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleUtils.WriteLineError("Not configured any profile");

            return Task.CompletedTask;
        }

        var lastActiveProfileName = _profilesRepository.ActiveName;

        var selectedProfileName = Prompt.Select(
            "Select profile",
            items: profileNames,
            defaultValue: lastActiveProfileName);
        
        var selectedProfileDo = SpinnerUtils.Run(
            () => _profilesRepository.GetByName(selectedProfileName),
            $"Read selected profile [{selectedProfileName}]");

        if (selectedProfileDo?.SsmPaths?.Any() != true)
        {
            ConsoleUtils.WriteLineError($"Not configured selected profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleUtils.WriteLineNotification($"Start de-activating profile [{lastActiveProfileName}]");

            var lastActiveProfileDo = 
                lastActiveProfileName == selectedProfileName
                ? selectedProfileDo
                : SpinnerUtils.Run(
                    () => _profilesRepository.GetByName(lastActiveProfileName),
                    $"Read last active profile [{lastActiveProfileName}]");

            if (lastActiveProfileDo != null)
            {
                var deletedEnvironmentVariables = SpinnerUtils.Run(
                    () => _environmentVariablesRepository.DeleteEnvironmentVariables(lastActiveProfileDo),
                    $"Delete last active profile [{lastActiveProfileName}] environment variables");
                
                deletedEnvironmentVariables.PrintEnvironmentVariables();
            }
        }

        _profilesRepository.ActiveName = selectedProfileName;
        ConsoleUtils.WriteLineNotification($"Start activating profile [{selectedProfileName}]");
        
        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        ssmParameters.PrintSsmParameters(selectedProfileDo.SsmPaths);
        
        var appliedEnvironmentVariables = SpinnerUtils.Run(
            () => _environmentVariablesRepository.SetEnvironmentVariables(
                ssmParameters,
                selectedProfileDo),
            $"Apply environment variables");
        
        appliedEnvironmentVariables.PrintEnvironmentVariables();
        
        ConsoleUtils.WriteLineInfo("DONE");

        return Task.CompletedTask;
    }
}