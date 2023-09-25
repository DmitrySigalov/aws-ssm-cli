namespace Aws.Ssm.Cli.SsmParameters.Rules;

public class SsmPathValidationRulesTests
{
    [Fact]
    public void Handle_ValidSsmPath_Check_SuccessResult()
    {
        var check = "/test";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            configuredSsmPaths);
        
        Assert.Null(result);
    }

    [Fact]
    public void Handle_EmptySsmPath_Check_FailResult()
    {
        var check = "";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            configuredSsmPaths);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void Handle_NotStartedFromKeyDelimeterSsmPath_Check_FailResult()
    {
        var check = "test";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            configuredSsmPaths);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void Handle_DuplicatedSsmPath_Check_FailResult()
    {
        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var check = configuredSsmPaths.First();

        var result = SsmPathValidationRules.Handle(
            check,
            configuredSsmPaths);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void Handle_DuplicatedParentSsmPath_Check_FailResult()
    {
        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var check = configuredSsmPaths.First() + "/test";

        var result = SsmPathValidationRules.Handle(
            check,
            configuredSsmPaths);
        
        Assert.NotNull(result);
    }

    [Fact]
    public void Handle_DuplicatedChildSsmPath_Check_FailResult()
    {
        var check = "/test";

        var configuredSsmPaths = new[]
        {
            check + "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            configuredSsmPaths);
        
        Assert.NotNull(result);
    }
}