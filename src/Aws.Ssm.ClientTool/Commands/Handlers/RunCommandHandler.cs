namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class RunCommandHandler : ICommandHandler
{
    public string Name => "run";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Not implemented command {Name}");

        return Task.CompletedTask;
    }
}