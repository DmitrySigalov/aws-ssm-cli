using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.UserSettings;
using Aws.Ssm.ClientTool.Utils;
using ConsoleTables;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ViewCommandHandler : ICommandHandler
{
    private readonly UserSettingsRepository _userSettingsRepository;

    private readonly SsmParametersRepository _ssmParametersRepository;
    
    private readonly EnvironmentRepository _environmentRepository;

    public ViewCommandHandler(
        UserSettingsRepository userSettingsRepository,
        SsmParametersRepository ssmParametersRepository,
        EnvironmentRepository environmentRepository)
    {
        _userSettingsRepository = userSettingsRepository;

        _ssmParametersRepository = ssmParametersRepository;
        
        _environmentRepository = environmentRepository;
    }
    
    public string Name => "view";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        var userSettings = SpinnerUtils.Run(
            _userSettingsRepository.Get,
            "Get user settings");

        PrintUserSettings(userSettings);

        if (userSettings.SsmPaths.Any() != true)
        {
            return Task.CompletedTask;
        }
        
        var pathsToView = Prompt.MultiSelect(
            $"- Select {nameof(userSettings.SsmPaths)} to view",
            userSettings.SsmPaths);

        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(pathsToView.ToHashSet()),
            "Get ssm parameters from AWS System Manager");

        PrintSsmParameters(ssmParameters, userSettings);

        return Task.CompletedTask;
    }

    private void PrintUserSettings(UserSettingsDo userSettings)
    {
        var table = new ConsoleTable("setting-name", "setting-value");
        table.Options.EnableCount = false;
        
        table.AddRow(
            nameof(userSettings.EnvironmentVariablePrefix), 
            userSettings.EnvironmentVariablePrefix);

        table.AddRow(
            nameof(userSettings.EnvironmentVariableDelimeter), 
            userSettings.EnvironmentVariableDelimeter);

        table.AddRow(
            nameof(userSettings.SsmPaths) + ".Count()", 
            userSettings.SsmPaths.Count);

        table.Write(Format.Minimal);
    }

    private void PrintSsmParameters(IDictionary<string, string> ssmParameters, UserSettingsDo userSettings)
    {
        var table = new ConsoleTable("ssm-param-name", "env-var-status", "ssm-param-value");
        table.Options.EnableCount = false;
        foreach (var resolvedParameterValue in ssmParameters)
        {
            var environmentVariableName = EnvironmentVariableNameConverter.ConvertFromSsmPath(
                resolvedParameterValue.Key,
                userSettings);
            var environmentVariableValue = _environmentRepository.GetEnvironmentVariable(
                environmentVariableName);

            var envVarStatus = "";
            if (environmentVariableValue == resolvedParameterValue.Value)
            {
                envVarStatus = "OK";
            }
            else if (!string.IsNullOrEmpty(environmentVariableValue))
            {
                envVarStatus = "Invalid";
            }
            else
            {
                envVarStatus = "None";
            }
            
            table.AddRow(
                resolvedParameterValue.Key, 
                envVarStatus,
                resolvedParameterValue.Value);
        }

        table.Write(Format.Minimal);
    }
}