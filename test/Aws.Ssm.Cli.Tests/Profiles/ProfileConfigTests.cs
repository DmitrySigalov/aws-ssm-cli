namespace Aws.Ssm.Cli.Profiles;

public class ProfileConfigTests
{
    [Fact]
    public void IsValid_Check()
    {
        var testComponent = new ProfileConfig
        {
            EnvironmentVariablePrefix = "",
            SsmPaths = null,
        };
        Assert.False(testComponent.IsValid);

        testComponent.SsmPaths = new HashSet<string>();
        Assert.False(testComponent.IsValid);

        testComponent.SsmPaths.Add("/test");
        Assert.True(testComponent.IsValid);

        testComponent.EnvironmentVariablePrefix = null;
        Assert.False(testComponent.IsValid);
    }
    
    [Fact]
    public void CopyFrom_Check()
    {
        var testComponent = new ProfileConfig
        {
            EnvironmentVariablePrefix = "UnexpectedPrefix",
            SsmPaths = new HashSet<string>
            {
                "/UnexpectedSsmPath"
            },
        };
        
        var testSource = new ProfileConfig
        {
            EnvironmentVariablePrefix = "NewPrefix",
            SsmPaths = new HashSet<string>
            {
                "/SsmPath1",
                "/SsmPath2",
            },
        };
        
        testComponent.CopyFrom(testSource);
        
        Assert.Equal(testSource.EnvironmentVariablePrefix, testComponent.EnvironmentVariablePrefix);
        Assert.True(testComponent.SsmPaths.SequenceEqual(testSource.SsmPaths));
    }
}