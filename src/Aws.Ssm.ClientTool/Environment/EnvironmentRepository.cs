namespace Aws.Ssm.ClientTool.Environment;

public class EnvironmentRepository
{
    public string GetEnvironmentVariable(string name)
    {
        Console.WriteLine("Not implemented");

        return null;
    }

    public void DeleteEnvironmentVariables(IEnumerable<string> baseNames)
    {
        foreach (var name in baseNames)
        {
            Console.WriteLine($"Not implemented: {name}");
        }
    }
}