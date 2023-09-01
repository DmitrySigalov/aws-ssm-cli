namespace Aws.Ssm.ClientTool.Profiles;

public class ProfilesRepository
{
    public string CurrentName { get; set; }
    
    public ISet<string> GetNames()
    {
        return new HashSet<string>();
    }

    public ProfileDo GetByName(string name)
    {
        Console.WriteLine("Stub");

        var result = new ProfileDo();

        result.SsmPaths.Add("/db/mysql");
        result.SsmPaths.Add("/message-broker/kafka");
        result.SsmPaths.Add("/message-broker/error");
        
        return result;
    }

    public void Save(string name, ProfileDo data)
    {
        Console.WriteLine("Not implemented");
    }
}