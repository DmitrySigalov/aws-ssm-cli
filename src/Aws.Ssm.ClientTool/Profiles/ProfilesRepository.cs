namespace Aws.Ssm.ClientTool.Profiles;

public class ProfilesRepository : IProfilesRepository
{
    public string ActiveName
    {
        get
        {
            return "Profile2";
        }
        set
        {
        }
    }
    
    public ISet<string> GetNames()
    {
        return new HashSet<string>
        {
            "Default",
            "Profile1",
            "Profile2",
            "Profile3WithError",
            "UnavailableProfile",
        };
    }

    public ProfileDo GetByName(string name)
    {
        if (name == "UnavailableProfile") return null;
        
        var result = new ProfileDo();

        result.SsmPaths.Add("/db/mysql/main");
        
        if (name == "Profile1") result.SsmPaths.Add("/message-broker/kafka/hermes");
        
        if (name == "Profile2") result.SsmPaths.Add("/message-broker/kafka/cdc");
        
        if (name == "Profile3WithError") result.SsmPaths.Add("/message-broker/error");
        
        return result;
    }

    public void Save(string name, ProfileDo data)
    {
    }
}