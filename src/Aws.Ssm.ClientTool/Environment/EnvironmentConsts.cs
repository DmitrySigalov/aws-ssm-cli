namespace Aws.Ssm.ClientTool.Environment;

public static class EnvironmentConsts
{
    public static EnvironmentVariableTarget EnvironmentVariableTarget => EnvironmentVariableTarget.User; 
    
    public static char VariableNameDelimeter => '_';
    
    public static char[] InvalidVariableNameCharacters => new [] { '/', '\\', ':', '-', };
}