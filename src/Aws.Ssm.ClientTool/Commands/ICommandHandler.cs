namespace Aws.Ssm.ClientTool.Commands;

public interface ICommandHandler
{
    string Name { get; }

    string Description { get; }

    Task Handle(CancellationToken cancellationToken);
}