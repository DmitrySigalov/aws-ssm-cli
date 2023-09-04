namespace Aws.Ssm.ClientTool.EnvironmentVariables;

public static class EnvironmentVariablesConsts
{
    public static string BaseVariableName => "AWS_SSM_CLI_TOOL"; 
    
    public static string GetClientToolVariableName(string name) => $"{BaseVariableName}_{name}".ToUpper(); 
    
    public static EnvironmentVariableTarget EnvironmentVariableTarget => EnvironmentVariableTarget.User; 
    
    public static char VariableNameDelimeter => '_';
    
    public static char[] InvalidVariableNameCharacters => new [] { '/', '\\', ':', '-', };
}