namespace Aws.Ssm.Cli.EnvironmentVariables;

public static class EnvironmentVariablesConsts
{
    private static string BaseVariableName => "AWS_SSM_CLI"; 
    
    public static string GetClientToolVariableName(string name) => $"{BaseVariableName}_{name}".ToUpper(); 
    
    public static char VariableNameDelimeter => '_';
    
    public static class FileNames
    {
        private static string Base => "export-list";

        public static string Descriptor => $"{Base}.json";
    
        public static string ScriptExtension
        {
            get
            {
                var shell = Environment.GetEnvironmentVariable("SHELL");

                if (shell?.Contains("zsh") == true)
                {
                    return ".zshrc";
                }
                
                if (shell?.Contains("bashrc") == true)
                {
                    return ".bashrc";
                }

                return ".unknown";
            }
        }

        public static string ScriptName => $"{Base}{ScriptExtension}";
    }
}