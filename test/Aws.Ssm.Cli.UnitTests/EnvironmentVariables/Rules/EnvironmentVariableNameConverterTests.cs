using Aws.Ssm.Cli.Profiles;

namespace Aws.Ssm.Cli.EnvironmentVariables.Rules;

public class EnvironmentVariableNameConverterTests
{
    [Fact]
    public void ConvertFromSsmPath_ConfiguredEmptyPrefix_Check_NoPrefix_UpperCase()
    {
        var profileConfig = new ProfileConfig
        {
            EnvironmentVariablePrefix = "",
        };

        var ssmPath = "/param1";

        var expectedResult = "PARAM1";
        
        var actualResult = EnvironmentVariableNameConverter.ConvertFromSsmPath(
            ssmPath,
            profileConfig);
        
        Assert.Equal(expectedResult, actualResult);
    }
    
    [Fact]
    public void ConvertFromSsmPath_ConfiguredPrefix_Check_Prefix_UpperCase()
    {
        var profileConfig = new ProfileConfig
        {
            EnvironmentVariablePrefix = "TEST_Prefix_",
        };

        var ssmPath = "/param1";

        var expectedResult = "TEST_PREFIX_PARAM1";
        
        var actualResult = EnvironmentVariableNameConverter.ConvertFromSsmPath(
            ssmPath,
            profileConfig);
        
        Assert.Equal(expectedResult, actualResult);
    }

    [Fact] 
    public void ConvertFromSsmPath_SpecialCharacters_Check_ReplacedSpecialCharacters_UpperCase()
    {
        var profileConfig = new ProfileConfig
        {
            EnvironmentVariablePrefix = "TEST_Prefix_",
        };

        var ssmPath = "/a/b{c}d\\e[f]g-h$i(j)k:l;m-n'o\"";

        var expectedResult = "TEST_PREFIX_A_B_C_D_E_F_G_H_I_J_K_L_M_N_O_";
        
        var actualResult = EnvironmentVariableNameConverter.ConvertFromSsmPath(
            ssmPath,
            profileConfig);
        
        Assert.Equal(expectedResult, actualResult);
    }
}