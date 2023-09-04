namespace Aws.Ssm.ClientTool.Runtime;

public class RuntimeParameters
{
    public string CommandName { get; set; }
    
    public bool IsDebug { get; set; }
    
    public string[] Args { get; set; }
}