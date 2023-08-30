namespace Ssm.Aws.ClientTool.Commands.Handlers;

public class HelpCommandHandler : ICommandHandler
{
    private readonly IEnumerable<string> _commandNames;
    
    public HelpCommandHandler(IEnumerable<ICommandHandler> commandHandlers)
    {
        _commandNames = commandHandlers
            .Select(x => x.Name)
            .ToHashSet();
    }
    
    public string Name => "help";

    public Task Handle(CancellationToken cancellationToken)
    {
        Console.WriteLine("Supported command(s):");
        Console.WriteLine($"- {Name}");
        foreach (var commandName in _commandNames)
        {
            Console.WriteLine($"- {commandName}");
        }

        return Task.CompletedTask;
    }
}