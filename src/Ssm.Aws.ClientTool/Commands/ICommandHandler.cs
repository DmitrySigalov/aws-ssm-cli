namespace Ssm.Aws.ClientTool.Commands;

public interface ICommandHandler
{
    string Name { get; }

    Task Handle(CancellationToken cancellationToken);
}