using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Utils;
using ConsoleTables;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class RunCommandHandler : ICommandHandler
{
    private readonly ProfilesRepository _profilesRepository;

    private readonly EnvironmentRepository _environmentRepository;
    
    private readonly SsmParametersRepository _ssmParametersRepository;

    public RunCommandHandler(
        ProfilesRepository profilesRepository,
        EnvironmentRepository environmentRepository,
        SsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentRepository = environmentRepository;

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
            ConsoleUtils.WriteLineError($"Not configured profile [{selectedProfileName}]");

            return Task.CompletedTask;
        }

        var ssmParameters = SpinnerUtils.Run(
            () => _ssmParametersRepository.GetDictionaryBy(selectedProfileDo.SsmPaths),
            "Get ssm parameters from AWS System Manager");

        if (ssmParameters.Any() != true)
        {
            ConsoleUtils.WriteLineError("No found any ssm parameters according to selected profile");

            return Task.CompletedTask;
        }

        if (!string.IsNullOrEmpty(lastActiveProfileName))
        {
            var lastActiveProfileDo = 
                lastActiveProfileName == selectedProfileName
                ? selectedProfileDo
                : SpinnerUtils.Run(
                    () => _profilesRepository.GetByName(lastActiveProfileName),
                    $"Read last active profile [{lastActiveProfileName}]");

            if (lastActiveProfileDo != null)
            {
                var deletedEnvironmentVariables = SpinnerUtils.Run(
                    () => DeleteLastActiveProfileEnvironmentVariables(lastActiveProfileDo),
                    $"Delete last active profile [{lastActiveProfileName}] environment variables");
                
                PrintDeletedEnvironmentVariableNames(deletedEnvironmentVariables);
            }
        }
        
        Console.WriteLine($"Not implemented command {Name}");

        return Task.CompletedTask;
    }

    private ISet<string> DeleteLastActiveProfileEnvironmentVariables(ProfileDo lastActiveProfileDo)
    {
        var convertedEnvironmentVariableBaseNames = lastActiveProfileDo.SsmPaths
            .Select(x => EnvironmentVariableNameConverter.ConvertFromSsmPath(x, lastActiveProfileDo))
            .ToArray();

        var environmentVariablesToDelete = _environmentRepository
            .GetEnvironmentVariableNames(convertedEnvironmentVariableBaseNames);

        if (environmentVariablesToDelete.Any() == false)
        {
            return new HashSet<string>();
        }
        
        _environmentRepository.DeleteEnvironmentVariables(environmentVariablesToDelete);

        return environmentVariablesToDelete;
    }

    private void PrintDeletedEnvironmentVariableNames(ISet<string> deletedEnvironmentVariables)
    {
        if (deletedEnvironmentVariables.Any() == false)
        {
            ConsoleUtils.WriteLineNotification("No found any environment variables");
            
            return;
        }
        
        var table = new ConsoleTable("deleted-environment-variable-name");
        table.Options.EnableCount = false;

        foreach (var envVar in deletedEnvironmentVariables)
        {
            table.AddRow(envVar);
        }
        
        table.Write(Format.Minimal);
    }
}