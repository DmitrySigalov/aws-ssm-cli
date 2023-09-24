namespace Okta.Aws.Cli.Abstractions;

public interface IVersionService
{
    public Task CheckAsync(CancellationToken cancellationToken);
}