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
    
    public ViewCommandHandler(
        UserSettingsRepository userSettingsRepository,
        SsmParametersRepository ssmParametersRepository)
    {
        _userSettingsRepository = userSettingsRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }
    
    public string Name => "view";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        var userSettings = SpinnerUtils.Run(
            _userSettingsRepository.Load,
            "Load configured user settings");
        
        Console.WriteLine("- Prefix: " + userSettings?.Prefix);
        Console.WriteLine("- Delimeter: " + userSettings?.Delimeter);

        if (userSettings?.Paths?.Any() != true)
        {
            Console.WriteLine("- Paths: not configured");

            return Task.CompletedTask;
        }
        
        var pathsToView = Prompt.MultiSelect(
            "Select paths to view",
            userSettings.Paths);

        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(pathsToView.ToHashSet()),
            "Get parameters from AWS System Manager");

        var invalidPaths = pathsToView
            .Where(x => ssmParameters.Keys.All(key => !key.StartsWith(x)))
            .ToList();
        if (invalidPaths.Any())
        {
            Console.WriteLine("Invalid paths:");
            invalidPaths.ForEach(x => Console.WriteLine($"- {x}"));
        }
        
        var table = new ConsoleTable("path", "value");
        table.Options.EnableCount = true;
        foreach (var resolvedParameterValue in ssmParameters)
        {
            table.AddRow(resolvedParameterValue.Key, resolvedParameterValue.Value);
        }
        table.Write(Format.Minimal);
        
        Console.WriteLine();

        return Task.CompletedTask;
    }
}