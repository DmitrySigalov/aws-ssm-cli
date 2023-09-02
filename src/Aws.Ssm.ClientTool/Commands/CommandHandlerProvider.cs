using Aws.Ssm.ClientTool.Commands.Handlers;
using Aws.Ssm.ClientTool.Utils;

namespace Aws.Ssm.ClientTool.Commands;

public class CommandHandlerProvider
{
    private readonly HelpCommandHandler _helpCommandHandler;

    public CommandHandlerProvider(
        IEnumerable<ICommandHandler> handlers,
        HelpCommandHandler helpCommandHandler)
    {
        handlers = handlers.ToArray();
        
        _helpCommandHandler = helpCommandHandler;
        
        All = new [] { _helpCommandHandler } 
            .Union(handlers)
            .ToDictionary(h => h.Name, h => h);
    }
    
    public IDictionary<string, ICommandHandler> All { get; }

    public ICommandHandler Get(string? commandName)
    {
        if (string.IsNullOrEmpty(commandName))
        {
            ConsoleUtils.WriteLineError("Undefined command argument");
            Console.WriteLine();
            
            return _helpCommandHandler;
        }

        if (!All.TryGetValue(commandName, out var handler))
        {
            ConsoleUtils.WriteLineError("Invalid command argument");
            Console.WriteLine();
            
            return _helpCommandHandler;
        }
        
        return handler;
    }
}