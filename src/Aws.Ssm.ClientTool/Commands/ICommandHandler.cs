namespace Aws.Ssm.ClientTool.Commands;

public interface ICommandHandler
{
    string Name { get; }

    string Help { get; }

    Task Handle(string[] args, CancellationToken cancellationToken);
}