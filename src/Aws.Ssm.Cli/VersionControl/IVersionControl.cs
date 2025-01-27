namespace Aws.Ssm.Cli.VersionControl;

public interface IVersionControl
{
    Task CheckVersionAsync(CancellationToken cancellationToken);
}