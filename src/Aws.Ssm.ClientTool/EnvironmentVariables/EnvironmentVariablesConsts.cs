namespace Aws.Ssm.ClientTool.EnvironmentVariables;

public static class EnvironmentVariablesConsts
{
    public static EnvironmentVariableTarget EnvironmentVariableTarget => EnvironmentVariableTarget.User; 
    
    public static char VariableNameDelimeter => '_';
    
    public static char[] InvalidVariableNameCharacters => new [] { '/', '\\', ':', '-', };
}