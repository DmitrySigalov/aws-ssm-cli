namespace Aws.Ssm.ClientTool.EnvironmentVariables;

public static class EnvironmentVariablesConsts
{
    private static string BaseVariableName => "AWS_SSM_CLI"; 
    
    public static string GetClientToolVariableName(string name) => $"{BaseVariableName}_{name}".ToUpper(); 
    
    public static EnvironmentVariableTarget EnvironmentVariableTarget => EnvironmentVariableTarget.User; 
    
    public static char VariableNameDelimeter => '_';
    
    public static char[] InvalidVariableNameCharacters => new [] { '/', '\\', ':', '-', };

    public static class FileNames
    {
        private static string Base => "environment-variables";

        public static string Descriptor => $"{Base}.json";
    
        public static string Script => $"{Base}.zshrc";
    }
}