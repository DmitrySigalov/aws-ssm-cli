namespace Aws.Ssm.ClientTool.Commands;

public interface ICommandHandler
{
    string Name { get; }

    Task Handle(CancellationToken cancellationToken);
}