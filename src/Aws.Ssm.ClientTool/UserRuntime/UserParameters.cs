namespace Aws.Ssm.ClientTool.UserRuntime;

public class UserParameters
{
    public string CommandName { get; set; }
    
    public bool IsDebug { get; set; }
    
    public string[] Args { get; set; }
}