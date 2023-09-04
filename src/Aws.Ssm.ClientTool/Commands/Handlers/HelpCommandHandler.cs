using Aws.Ssm.ClientTool.Helpers;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class HelpCommandHandler : ICommandHandler
{
    private readonly ISet<ICommandHandler> _commandHandlers;

    public HelpCommandHandler(IEnumerable<ICommandHandler> commandHandlers)
    {
        _commandHandlers = new[] { this }
            .Union(commandHandlers)
            .ToHashSet();
    }

    public string Name => "help";

    public string Description => "?";

    public Task Handle(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"Supported commands:");

        var table = new ConsoleTable("command-name", "help");
        table.Options.EnableCount = false;

        foreach (var commandHandler in _commandHandlers)
        {
            table.AddRow(commandHandler.Name, commandHandler.Description);
        }

        table.Write(Format.Minimal);

        return Task.CompletedTask;
    }
}