using Aws.Ssm.ClientTool.Commands.Handlers;
using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.UserRuntime;
using Sharprompt;

namespace Aws.Ssm.ClientTool.Commands;

public class CommandHandlerProvider
{
    private readonly UserParameters _userParameters;
    
    private readonly HelpCommandHandler _helpCommandHandler;

    private readonly ICommandHandler _defaultCommandHandler;

    private readonly IDictionary<string, ICommandHandler> _allCommandHandlersByFullName;

    private readonly IDictionary<string, ICommandHandler> _allCommandHandlersByShortName;

    public CommandHandlerProvider(
        UserParameters userParameters,
        IEnumerable<ICommandHandler> handlers,
        HelpCommandHandler helpCommandHandler)
    {
        _userParameters = userParameters;
        
        handlers = handlers.ToArray();

        _defaultCommandHandler = handlers.FirstOrDefault();
        
        _helpCommandHandler = helpCommandHandler;

        _allCommandHandlersByFullName = new [] { _helpCommandHandler } 
            .Union(handlers)
            .ToDictionary(h => h.BaseName, h => h);

        _allCommandHandlersByShortName = _allCommandHandlersByFullName
            .Values
            .Where(x => !string.IsNullOrWhiteSpace(x.ShortName))
            .ToDictionary(
                x => x.ShortName,
                y => y);
    }
    
    public ICommandHandler Get()
    {
        var commandName = _userParameters.CommandName;
        
        if (commandName=="*" || string.IsNullOrEmpty(commandName))
        {
            commandName = Prompt.Select(
                "Select command",
                _allCommandHandlersByFullName.Select(x => x.Key),
                defaultValue: _defaultCommandHandler?.BaseName);
        }

        if (!_allCommandHandlersByFullName.TryGetValue(commandName, out var handler) &&
            !_allCommandHandlersByShortName.TryGetValue(commandName, out handler))
        {
            ConsoleHelper.WriteLineError("Invalid command argument");
            Console.WriteLine();
            
            return _helpCommandHandler;
        }
        
        return handler;
    }
}