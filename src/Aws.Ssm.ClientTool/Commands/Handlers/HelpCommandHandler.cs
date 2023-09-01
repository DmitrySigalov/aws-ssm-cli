using ConsoleTables;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

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
        var table = new ConsoleTable("Supported commands");
        table.Options.EnableCount = false;

        table.AddRow(Name);

        foreach (var commandName in _commandNames.OrderBy(x => x))
        {
            table.AddRow(commandName);
        }

        table.Write(Format.Minimal);

        return Task.CompletedTask;
    }
}