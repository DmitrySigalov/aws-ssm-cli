using Ssm.Aws.ClientTool.Commands.Handlers;

namespace Ssm.Aws.ClientTool.Commands;

public class CommandHandlerProvider
{
    private readonly IDictionary<string, ICommandHandler> _handlers;
    private readonly HelpCommandHandler _helpCommandHandler;

    public CommandHandlerProvider(IEnumerable<ICommandHandler> handlers)
    {
        handlers = handlers.ToArray();
        
        _helpCommandHandler = new HelpCommandHandler(handlers);
        
        _handlers = new [] { _helpCommandHandler } 
            .Union(handlers)
            .ToDictionary(h => h.Name, h => h);
    }

    public ICommandHandler Get(string? commandName)
    {
        if (string.IsNullOrEmpty(commandName))
        {
            Console.WriteLine("Select command argument");
            
            return _helpCommandHandler;
        }

        if (!_handlers.TryGetValue(commandName, out var handler))
        {
            Console.WriteLine("Invalid command argument");
            
            return _helpCommandHandler;
        }

        return handler;
    }
}