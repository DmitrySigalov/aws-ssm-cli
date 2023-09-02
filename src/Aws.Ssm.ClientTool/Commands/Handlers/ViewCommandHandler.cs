using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Utils;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ViewCommandHandler : ICommandHandler
{
    private readonly IProfilesRepository _profilesRepository;

    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;

    public ViewCommandHandler(
        IProfilesRepository profilesRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentVariablesRepository = environmentVariablesRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "view";
    
    public string Help => "View profile configuration";

    public Task Handle(string[] args, CancellationToken cancellationToken)
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

        var selectedProfileName = 
            profileNames.Count == 1
            ? profileNames.Single()
            : Prompt.Select(
                "Select profile to view",
                items: profileNames,
                defaultValue: lastActiveProfileName);

        var selectedProfileDo = SpinnerUtils.Run(
            () => _profilesRepository.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");
        
        selectedProfileDo?.PrintProfileSettings();

        if (selectedProfileDo?.SsmPaths?.Any() != true)
        {
            ConsoleUtils.WriteLineError($"Not configured profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }
        
        var resolvedSsmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(selectedProfileDo);

        var actualEnvironmentVariables = SpinnerUtils.Run(
            () => _environmentVariablesRepository.GetAll(selectedProfileDo),
            "Get environment variables");

        actualEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidation(
            resolvedSsmParameters,
            selectedProfileDo);

        ConsoleUtils.WriteLineInfo($"DONE");

        return Task.CompletedTask;
    }
}