using Aws.Ssm.ClientTool.EnvironmentVariables;
using Aws.Ssm.ClientTool.EnvironmentVariables.Extensions;
using Aws.Ssm.ClientTool.EnvironmentVariables.Rules;
using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Profiles.Extensions;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.SsmParameters.Extensions;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ViewCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    
    private readonly ISsmParametersProvider _ssmParametersProvider;

    public ViewCommandHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISsmParametersProvider ssmParametersProvider)
    {
        _profileConfigProvider = profileConfigProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _ssmParametersProvider = ssmParametersProvider;
    }
    
    public string BaseName => "view";
    
    public string ShortName => "";

    public string Description => "View configuration";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"START - {Description}");
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
                "Select profile",
                items: profileNames,
                defaultValue: lastActiveProfileName);

        var selectedProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(selectedProfileName),
            $"Read profile [{selectedProfileName}]");
        
        selectedProfileDo?.PrintProfileSettings();

        if (selectedProfileDo?.IsValid != true)
        {
            ConsoleHelper.WriteLineError($"Not configured profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }
        
        var resolvedSsmParameters = SpinnerHelper.Run(
            () => _ssmParametersProvider.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");

        resolvedSsmParameters.PrintSsmParameters(selectedProfileDo);
        
        resolvedSsmParameters.PrintSsmParameterToEnvironmentVariableNamesMapping(
            selectedProfileDo);
        
        var convertedEnvironmentVariables = resolvedSsmParameters
            .Select(x => new
            {
                Name = EnvironmentVariableNameConverter.ConvertFromSsmPath(x.Key, selectedProfileDo),
                Value = x.Value,
            })
            .GroupBy(x => x.Name)
            .ToDictionary(
                x => x.Key,
                x => x.Last().Value);

        ConsoleHelper.WriteLineNotification("Launch settings json:");
        Console.WriteLine(JsonSerializationHelper.Serialize(
            new
            {
                profiles = new
                {
                    profileName = new
                    {
                        environmentVariables = convertedEnvironmentVariables,
                    }
                }
            }));
        Console.WriteLine();

        convertedEnvironmentVariables.PrintInvalidEnvironmentVariables(
            resolvedSsmParameters,
            selectedProfileDo);

        ConsoleHelper.WriteLineInfo($"DONE - {Description} with profile [{selectedProfileName}] configuration");

        return Task.CompletedTask;
    }
}