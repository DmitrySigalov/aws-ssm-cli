namespace Aws.Ssm.Cli.Helpers;

public static class ShellHelper
{
    public static string GetShellScriptFileName()
    {
        var shell = Environment.GetEnvironmentVariable("SHELL");

        if (shell?.Contains("zsh") == true)
        {
            return ".zshrc";
        }

        return ".bashrc";
    }
}