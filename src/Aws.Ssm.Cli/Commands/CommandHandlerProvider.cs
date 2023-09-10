using Aws.Ssm.Cli.Commands.Handlers;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.UserRuntime;
using Sharprompt;

namespace Aws.Ssm.Cli.Commands;

public class CommandHandlerProvider
{
    private readonly UserParameters _userParameters;
    
    private readonly HelpCommandHandler _helpCommandHandler;

    private readonly ICommandHandler _defaultCommandHandler;

    private readonly IDictionary<string, ICommandHandler> _allCommandHandlers;

    public CommandHandlerProvider(
        UserParameters userParameters,
        IEnumerable<ICommandHandler> handlers,
        HelpCommandHandler helpCommandHandler)
    {
        _userParameters = userParameters;
        
        handlers = handlers.ToArray();

        _defaultCommandHandler = handlers.FirstOrDefault();
        
        _helpCommandHandler = helpCommandHandler;

        _allCommandHandlers = new [] { _helpCommandHandler } 
            .Union(handlers)
            .ToDictionary(h => h.CommandName, h => h);
    }
    
    public ICommandHandler Get()
    {
        var commandName = _userParameters.CommandName;
        
        if (commandName=="*" || string.IsNullOrEmpty(commandName))
        {
            commandName = Prompt.Select(
                "Select command",
                _allCommandHandlers.Select(x => x.Key),
                defaultValue: _defaultCommandHandler?.CommandName);
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