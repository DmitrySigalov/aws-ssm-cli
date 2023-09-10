using ConsoleTables;

namespace Aws.Ssm.Cli.Profiles.Extensions;

public static class ConsoleOutputExtensions
{
    public static void PrintProfileSettings(this ProfileConfig profileConfig)
    {
        if (profileConfig == null)
        {
            return;
        }
        
        var table = new ConsoleTable("setting-name", "setting-value");

        table.AddRow(nameof(profileConfig.EnvironmentVariablePrefix), profileConfig.EnvironmentVariablePrefix);

        table.AddRow(nameof(profileConfig.SsmPaths) + ".Count()", profileConfig.SsmPaths?.Count ?? 0);
        if (profileConfig.SsmPaths != null)
        {
            var index = 0;
            foreach (var ssmPath in profileConfig.SsmPaths.OrderBy(x => x))
            {
                table.AddRow(
                    $"{nameof(profileConfig.SsmPaths)}[{index++}]", 
                    ssmPath);
            }
        }

        table.Write(Format.Minimal);    
    }
}