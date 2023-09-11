using Aws.Ssm.Cli.Helpers;

namespace Aws.Ssm.Cli.Profiles.Extensions;

public static class ConsoleOutputExtensions
{
    public static void PrintProfileSettings(this ProfileConfig profileConfig)
    {
        if (profileConfig == null)
        {
            return;
        }
        
        var data = JsonSerializationHelper.Serialize(profileConfig);
        
        Console.WriteLine(data);
        Console.WriteLine();
    }
}