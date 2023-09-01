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

    private readonly ISsmParametersRepository _ssmParametersRepository;
    
    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;

    public ViewCommandHandler(
        UserSettingsRepository userSettingsRepository,
        ISsmParametersRepository ssmParametersRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository)
    {
        _userSettingsRepository = userSettingsRepository;

        _ssmParametersRepository = ssmParametersRepository;
        
        _environmentVariablesRepository = environmentVariablesRepository;
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
        
        var ssmPathsToView = Prompt.MultiSelect(
            $"- Select {nameof(userSettings.SsmPaths)} to view",
            userSettings.SsmPaths);

        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(ssmPathsToView.ToHashSet()),
            "Get ssm parameters from AWS System Manager");

        PrintSsmParameters(ssmParameters, userSettings);

        PrintNoFoundSsmPaths(ssmParameters, ssmPathsToView);

        return Task.CompletedTask;
    }

    private void PrintUserSettings(UserSettingsDo userSettings)
    {
        var table = new ConsoleTable("setting-name", "setting-value");
        table.Options.EnableCount = false;
        
        table.AddRow(
            nameof(userSettings.SsmPaths) + ".Count()", 
            userSettings.SsmPaths.Count);

        table.AddRow(
            nameof(userSettings.EnvironmentVariablePrefix), 
            userSettings.EnvironmentVariablePrefix);

        table.AddRow(
            nameof(userSettings.EnvironmentVariableDelimeter), 
            userSettings.EnvironmentVariableDelimeter);

        table.AddRow(
            nameof(userSettings.EnvironmentVariableNamingType), 
            userSettings.EnvironmentVariableNamingType);

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
            var environmentVariableValue = _environmentVariablesRepository.Get(
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
    
    private void PrintNoFoundSsmPaths(IDictionary<string, string> ssmParameters, IEnumerable<string> ssmPathsToView)
    {
        var invalidPaths = ssmPathsToView
            .Where(x => ssmParameters.Keys.All(y => !y.StartsWith(x)))
            .ToArray();

        var table = new ConsoleTable("unavailable-ssm-paths");
        table.Options.EnableCount = false;
        foreach (var ssmPath in invalidPaths)
        {
            table.AddRow(ssmPath);
        }

        table.Write(Format.Minimal);
    }
}