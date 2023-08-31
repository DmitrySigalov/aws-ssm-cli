namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class ViewCommandHandler : ICommandHandler
{
    public string Name => "view";
    
    public Task Handle(CancellationToken cancellationToken)
    {
        Console.WriteLine($"Not implemented command {Name}");

        return Task.CompletedTask;
    }
}