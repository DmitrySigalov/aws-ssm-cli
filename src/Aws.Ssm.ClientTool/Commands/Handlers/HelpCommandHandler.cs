using Aws.Ssm.ClientTool.Helpers;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.Commands.Handlers;

public class HelpCommandHandler : ICommandHandler
{
    private readonly ISet<ICommandHandler> _commandHandlers;
    
    public HelpCommandHandler(IEnumerable<ICommandHandler> commandHandlers)
    {
        _commandHandlers =new [] { this } 
            .Union(commandHandlers)
            .ToHashSet();
    }
    
    public string Name => "help";

    public string Help => "?";

    public Task Handle(string[] args, CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineNotification($"Process [{Name}] command");

        ConsoleHelper.Notification(() =>
        {
            var table = new ConsoleTable("command-name", "help");
            table.Options.EnableCount = false;

            foreach (var commandHandler in _commandHandlers)
            {
                table.AddRow(commandHandler.Name, commandHandler.Help);
            }

            table.Write(Format.Minimal);
        });

        return Task.CompletedTask;
    }
}