using Aws.Ssm.ClientTool.EnvironmentVariables;
using Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Helpers;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ViewCommandHandler : ICommandHandler
{
    private readonly IProfilesRepository _profilesRepository;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;

    public ViewCommandHandler(
        IProfilesRepository profilesRepository,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentVariablesProvider = environmentVariablesProvider;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "view";
    
    public string Description => "View profile configuration and environment state";

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
                "Select profile to view",
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
        
        var resolvedSsmParameters = SpinnerHelper.Run(
            () => _ssmParametersRepository.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(selectedProfileDo);

        var actualEnvironmentVariables = SpinnerHelper.Run(
            () => _environmentVariablesProvider.GetAll(selectedProfileDo),
            "Get environment variables");

        actualEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidation(
            resolvedSsmParameters,
            selectedProfileDo);

        ConsoleHelper.WriteLineInfo($"DONE - Viewed profile [{selectedProfileName}]");

        return Task.CompletedTask;
    }
}