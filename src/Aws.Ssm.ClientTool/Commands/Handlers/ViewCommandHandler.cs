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

        Console.WriteLine($"- {nameof(userSettings.EnvVarNamePrefix)}: " + userSettings.EnvVarNamePrefix);
        Console.WriteLine($"- {nameof(userSettings.EnvVarNameDelimeter)}: " + userSettings.EnvVarNameDelimeter);

        if (userSettings.SsmPaths.Any() != true)
        {
            Console.WriteLine($"- {nameof(userSettings.SsmPaths)}: not configured");

            return Task.CompletedTask;
        }
        
        var pathsToView = Prompt.MultiSelect(
            $"- Select {nameof(userSettings.SsmPaths)} to view",
            userSettings.SsmPaths);

        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(pathsToView.ToHashSet()),
            "Get ssm parameters from AWS System Manager");

        var invalidPaths = pathsToView
            .Where(x => ssmParameters.Keys.All(key => !key.StartsWith(x)))
            .ToList();
        if (invalidPaths.Any())
        {
            Console.WriteLine($"Invalid {nameof(userSettings.SsmPaths)}:");
            invalidPaths.ForEach(x => Console.WriteLine($"- {x}"));
        }

        PrintSsmParameters(ssmParameters, userSettings);

        return Task.CompletedTask;
    }

    private void PrintSsmParameters(IDictionary<string, string> ssmParameters, UserSettingsDo userSettings)
    {
        var table = new ConsoleTable("ssm-param-name", "env-var-status", "ssm-param-value");
        table.Options.EnableCount = true;
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
                envVarStatus = "Failed";
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

        Console.WriteLine();
        table.Write(Format.Minimal);
        Console.WriteLine();
    }
}