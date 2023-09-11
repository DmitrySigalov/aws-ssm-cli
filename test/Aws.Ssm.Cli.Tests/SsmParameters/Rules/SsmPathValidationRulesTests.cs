using Moq;

namespace Aws.Ssm.Cli.SsmParameters.Rules;

public class SsmPathValidationRulesTests
{
    private readonly Mock<ISsmParametersProvider> _mockISsmParametersProvider;

    public SsmPathValidationRulesTests()
    {
        _mockISsmParametersProvider = new Mock<ISsmParametersProvider>();
    }

    [Fact]
    public void Handle_ValidSsmPath_Check_SuccessResult_CallSsmParametersProvider()
    {
        var check = "/test";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        _mockISsmParametersProvider
            .Setup(x => x.GetDictionaryBy(new HashSet<string> { check, }))
            .Returns(new Dictionary<string, string>
            {
                { check + "/param1", "value1" },
            });

        var result = SsmPathValidationRules.Handle(
            check,
            _mockISsmParametersProvider.Object,
            configuredSsmPaths);
        
        Assert.Null(result);
        
        _mockISsmParametersProvider.Verify(
            x => x.GetDictionaryBy(new HashSet<string> { check, }),
            Times.Once);
    }

    [Fact]
    public void Handle_EmptySsmPath_Check_FailResult_NoCallSsmParametersProvider()
    {
        var check = "";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            _mockISsmParametersProvider.Object,
            configuredSsmPaths);
        
        Assert.NotNull(result);
        
        _mockISsmParametersProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_NotStartedFromKeyDelimeterSsmPath_Check_FailResult_NoCallSsmParametersProvider()
    {
        var check = "test";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            _mockISsmParametersProvider.Object,
            configuredSsmPaths);
        
        Assert.NotNull(result);
        
        _mockISsmParametersProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_DuplicatedParentParentSsmPath_Check_FailResult_NoCallSsmParametersProvider()
    {
        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var check = configuredSsmPaths.First() + "/test";

        var result = SsmPathValidationRules.Handle(
            check,
            _mockISsmParametersProvider.Object,
            configuredSsmPaths);
        
        Assert.NotNull(result);
        
        _mockISsmParametersProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_DuplicatedChildParentSsmPath_Check_FailResult_NoCallSsmParametersProvider()
    {
        var check = "/test";

        var configuredSsmPaths = new[]
        {
            check + "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            _mockISsmParametersProvider.Object,
            configuredSsmPaths);
        
        Assert.NotNull(result);
        
        _mockISsmParametersProvider.VerifyNoOtherCalls();
    }

    [Fact]
    public void Handle_UnavailableSsmPath_Check_FailResult_CallSsmParametersProvider()
    {
        var check = "/test";

        var configuredSsmPaths = new[]
        {
            "/configuredSsmPath1",
            "/configuredSsmPath2",
        };

        var result = SsmPathValidationRules.Handle(
            check,
            _mockISsmParametersProvider.Object,
            configuredSsmPaths);
        
        Assert.NotNull(result);
        
        _mockISsmParametersProvider.Verify(
            x => x.GetDictionaryBy(new HashSet<string> { check, }),
            Times.Once);
    }
}