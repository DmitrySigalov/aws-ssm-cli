namespace Ssm.Aws.ClientTool.Commands.Handlers;

public class ConfigCommandHandler : ICommandHandler
{
    public string Name => "config";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Not implemented command {Name}");

        return Task.CompletedTask;
    }
}