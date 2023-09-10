namespace Aws.Ssm.Cli.Commands;

public interface ICommandHandler
{
    string BaseName { get; }

    string ShortName { get; }

    string Description { get; }

    Task Handle(CancellationToken cancellationToken);
}