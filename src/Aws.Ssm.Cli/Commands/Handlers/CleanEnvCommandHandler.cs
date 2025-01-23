using Aws.Ssm.Cli.EnvironmentVariables;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles;
using Aws.Ssm.Cli.SsmParameters;
using Aws.Ssm.Cli.EnvironmentVariables.Extensions;
using Aws.Ssm.Cli.Profiles.Extensions;

namespace Aws.Ssm.Cli.Commands.Handlers;

public class CleanEnvCommandHandler : ICommandHandler
{
    private readonly IProfileConfigProvider _profileConfigProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
    
    private readonly ISsmParametersProvider _ssmParametersProvider;

    public CleanEnvCommandHandler(
        IProfileConfigProvider profileConfigProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ISsmParametersProvider ssmParametersProvider)
    {
        _profileConfigProvider = profileConfigProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _ssmParametersProvider = ssmParametersProvider;
    }
    
    public string CommandName => "clean-env";

    public string Description => "Clean environment variables";

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
        if (string.IsNullOrEmpty(lastActiveProfileName))
        {
            ConsoleHelper.WriteLineInfo($"No active profile");

            return Task.CompletedTask;
        }

        var lastActiveProfileDo = SpinnerHelper.Run(
            () => _profileConfigProvider.GetByName(lastActiveProfileName),
            $"Read profile [{lastActiveProfileName}]");

        if (lastActiveProfileDo?.IsValid != true)
        {
            ConsoleHelper.WriteLineError($"Not found profile [{lastActiveProfileName}] configuration");

            return Task.CompletedTask;
        }

        lastActiveProfileDo.PrintProfileSettings();

        SpinnerHelper.Run(
            () => _environmentVariablesProvider.DeleteAll(lastActiveProfileDo),
            "Delete current active environment variables");

        _profileConfigProvider.ActiveName = null;
        
        ConsoleHelper.WriteLineInfo($"DONE - Deactivated profile [{lastActiveProfileDo}]");

        var activationNotes = _environmentVariablesProvider.CompleteActivationEnvironmentVariables();
        if (!string.IsNullOrWhiteSpace(activationNotes))
        {
            Console.WriteLine(activationNotes);
        }

        return Task.CompletedTask;
    }
}