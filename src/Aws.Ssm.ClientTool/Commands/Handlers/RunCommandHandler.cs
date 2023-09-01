using Aws.Ssm.ClientTool.Profiles;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class RunCommandHandler : ICommandHandler
{
    private readonly ProfilesRepository _profilesRepository;

    public RunCommandHandler()
    {
        
    }
    
    public string Name => "run";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Not implemented command {Name}");

        return Task.CompletedTask;
    }
}