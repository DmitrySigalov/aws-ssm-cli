using Aws.Ssm.Cli.Helpers;

namespace Okta.Aws.Cli.Abstractions.Services;

public class VersionService : IVersionService
{
    public Task CheckAsync(CancellationToken cancellationToken)
    {
        ConsoleHelper.WriteLineError($"{nameof(VersionService)} still not implemented!");

        return Task.CompletedTask;
    }
}