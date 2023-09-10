using Aws.Ssm.Cli.EnvironmentVariables;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.SsmParameters;
using Aws.Ssm.Cli.EnvironmentVariables.Extensions;
using Aws.Ssm.Cli.Profiles.Extensions;
using Aws.Ssm.Cli.SsmParameters.Extensions;
using Sharprompt;

namespace Aws.Ssm.Cli.Commands.Handlers;

public class SetEnvCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    
    private readonly ISsmParametersProvider _ssmParametersProvider;

    public SetEnvCommandHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISsmParametersProvider ssmParametersProvider)
    {
        _profileConfigProvider = profileConfigProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _ssmParametersProvider = ssmParametersProvider;
    }
    
    public string CommandName => "set";
    
    public string ShortName => "";

    public string Description => "Set environment variables";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification(Description);
        Console.WriteLine();

        var profileNames = SpinnerHelper.Run(
            _profileConfigProvider.GetNames,
            "Get profile names");

        if (profileNames.Any() == false)
        {
            ConsoleHelper.WriteLineError("Not configured any profile");

            return Task.CompletedTask;
        }

        var lastActiveProfileName = _profileConfigProvider.ActiveName;
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
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");

        if (selectedProfileDo?.IsValid != true)
        {
            ConsoleHelper.WriteLineError($"Not configured profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        selectedProfileDo.PrintProfileSettings();

        var resolvedSsmParameters = SpinnerHelper.Run(
            () => _ssmParametersProvider.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");
        
        resolvedSsmParameters.PrintSsmParameters(selectedProfileDo);
        if (resolvedSsmParameters.Any() == false)
        {
            ConsoleHelper.WriteLineError("NOT DONE - Unavailable ssm parameters");

            return Task.CompletedTask;
        }

        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            var lastActiveProfileDo = 
                lastActiveProfileName == selectedProfileName
                    ? selectedProfileDo
                    : SpinnerHelper.Run(
                        () => _profileConfigProvider.GetByName(lastActiveProfileName),
                        $"Read current active profile [{lastActiveProfileName}]");

            if (lastActiveProfileDo?.IsValid == true)
            {
                SpinnerHelper.Run(
                    () => _environmentVariablesProvider.DeleteAll(lastActiveProfileDo),
                    "Delete current active environment variables");
            }
        }

        _profileConfigProvider.ActiveName = selectedProfileName;
        
        var appliedEnvironmentVariables = SpinnerHelper.Run(
            () => _environmentVariablesProvider.SetFromSsmParameters(
                resolvedSsmParameters,
                selectedProfileDo),
            $"Apply new environment variables");
        
        resolvedSsmParameters.PrintSsmParameterToEnvironmentVariableNamesMapping(
            selectedProfileDo);
        
        appliedEnvironmentVariables.PrintEnvironmentVariablesWithSsmParametersValidationStatus(
            resolvedSsmParameters,
            selectedProfileDo);
        
        ConsoleHelper.WriteLineInfo($"DONE - {Description} with profile [{selectedProfileName}]");

        return Task.CompletedTask;
    }
}