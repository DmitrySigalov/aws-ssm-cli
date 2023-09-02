using Aws.Ssm.ClientTool.Environment;
using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.SsmParameters;
using Aws.Ssm.ClientTool.Utils;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ConfigCommandHandler : ICommandHandler
{
    private readonly IProfilesRepository _profilesRepository;

    private readonly IEnvironmentVariablesRepository _environmentVariablesRepository;
    
    private readonly ISsmParametersRepository _ssmParametersRepository;

    public ConfigCommandHandler(
        IProfilesRepository profilesRepository,
        IEnvironmentVariablesRepository environmentVariablesRepository,
        ISsmParametersRepository ssmParametersRepository)
    {
        _profilesRepository = profilesRepository;

        _environmentVariablesRepository = environmentVariablesRepository;

        _ssmParametersRepository = ssmParametersRepository;
    }

    public string Name => "config";

    public string Help => "Create/update/delete profile configuration";

    public Task Handle(string[] args, CancellationToken cancellationToken)
    {
        ConsoleUtils.WriteLineNotification($"Process [{Name}] command");
        Console.WriteLine();

        var profileNames = SpinnerUtils.Run(
            _profilesRepository.GetNames,
            "Get profile names");

        return Task.CompletedTask;
    }
}