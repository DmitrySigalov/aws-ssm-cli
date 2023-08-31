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
            "Load user settings");
        
        Console.WriteLine("Configured user settings:");
        Console.WriteLine("- Prefix: " + userSettings?.Prefix);
        Console.WriteLine("- Delimeter: " + userSettings?.Delimeter);
        Console.WriteLine("- Paths: " + (userSettings?.Paths?.Count ?? 0) + " items");

        if (userSettings?.Paths?.Any() != true)
        {
            return Task.CompletedTask;
        }
        
        var pathsToView = Prompt.MultiSelect(
            "Select paths to view",
            userSettings.Paths,
            defaultValues: userSettings.Paths);

        var ssmParameters = SpinnerUtils.Run(
            () =>_ssmParametersRepository.GetDictionaryBy(pathsToView.ToHashSet()),
            "Search ssm parameters by configured paths");
        
        var table = new ConsoleTable("path", "status", "value");
        
        table.Write();
        Console.WriteLine();

        return Task.CompletedTask;
    }
}