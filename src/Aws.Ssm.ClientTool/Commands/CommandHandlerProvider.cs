using Aws.Ssm.ClientTool.Commands.Handlers;
using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.Runtime;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands;

public class CommandHandlerProvider
{
    private readonly RuntimeParameters _runtimeParameters;
    
    private readonly HelpCommandHandler _helpCommandHandler;

    private readonly ICommandHandler _defaultCommandHandler;

    private readonly IDictionary<string, ICommandHandler> _allCommandHandlers;

    public CommandHandlerProvider(
        RuntimeParameters runtimeParameters,
        IEnumerable<ICommandHandler> handlers,
        HelpCommandHandler helpCommandHandler)
    {
        _runtimeParameters = runtimeParameters;
        
        handlers = handlers.ToArray();

        _defaultCommandHandler = handlers.FirstOrDefault();
        
        _helpCommandHandler = helpCommandHandler;
        
        _allCommandHandlers = new [] { _helpCommandHandler } 
            .Union(handlers)
            .ToDictionary(h => h.Name, h => h);
    }
    
    public ICommandHandler Get()
    {
        var commandName = _runtimeParameters.CommandName;
        
        if (commandName=="*" || string.IsNullOrEmpty(commandName))
        {
            commandName = Prompt.Select(
                "Select command",
                _allCommandHandlers.Select(x => x.Key),
                defaultValue: _defaultCommandHandler?.Name);
        }

        if (!_allCommandHandlers.TryGetValue(commandName, out var handler))
        {
            ConsoleHelper.WriteLineError("Invalid command argument");
            Console.WriteLine();
            
            return _helpCommandHandler;
        }
        
        return handler;
    }
}