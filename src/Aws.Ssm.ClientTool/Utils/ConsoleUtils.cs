namespace Aws.Ssm.ClientTool.Utils;

public static class ConsoleUtils
{
    public static void WriteLineError(string text)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(text);
        Console.ResetColor();
    }
    
    public static void WriteLineNotification(string text)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(text);
        Console.ResetColor();
    }
}