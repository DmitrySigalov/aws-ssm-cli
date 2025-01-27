namespace Aws.Ssm.Cli.GitHub;

public interface IGitHubClient
{
    Task<GitHubModel.Response<GitHubModel.Release>> GetLatestReleaseAsync(CancellationToken cancellationToken);
}