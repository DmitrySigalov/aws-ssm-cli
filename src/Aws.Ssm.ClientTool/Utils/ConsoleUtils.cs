using System.Drawing;

namespace Aws.Ssm.ClientTool.Utils;

public static class ConsoleUtils
{
    public static void WriteLineInfo(string text) 
        => HandleInfo(() => Console.WriteLine(text));
    
    public static void WriteLineError(string text) 
        => HandleError(() => Console.WriteLine(text));
    
    public static void WriteLineNotification(string text)
        => HandleNotification(() => Console.WriteLine(text));
    
    public static void HandleInfo(Action action)
        => Handle(action, ConsoleColor.Green);
    
    public static void HandleError(Action action)
        => Handle(action, ConsoleColor.Red);
    
    public static void HandleNotification(Action action)
        => Handle(action, ConsoleColor.Yellow);
    
    private static void Handle(Action action, ConsoleColor foregroundColor)
    {
        Console.ForegroundColor = foregroundColor;
        action();
        Console.ResetColor();
    }
}