namespace Aws.Ssm.Cli.Commands;

public interface ICommandHandler
{
    string CommandName { get; }

    string Description { get; }

    Task Handle(CancellationToken cancellationToken);
}