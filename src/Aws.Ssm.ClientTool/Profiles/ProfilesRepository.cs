namespace Aws.Ssm.ClientTool.Profiles;

public class ProfilesRepository : IProfilesRepository
{
    public string ActiveName
    {
        get
        {
            return GetNames().FirstOrDefault();
        }
        set
        {
        }
    }
    
    public ISet<string> GetNames()
    {
        return new HashSet<string>
        {
            "default",
            // "profile1",
            // "profile2",
            // "profile3WithAddedMissingSsmParameter",
            // "profile4WithMissingSsmParameterOnly",
            // "unavailableProfile",
        };
    }

    public ProfileDo GetByName(string name)
    {
        if (name == "unavailableProfile") return null;
        
        var result = new ProfileDo();

        result.EnvironmentVariablePrefix = "SSM_";
        
        if (name.Contains( "profile1"))
        {
            result.EnvironmentVariablePrefix = "SSM-";
        }
        else if (name.Contains( "profile2"))
        {
            result.EnvironmentVariablePrefix = "";
        }

        if (!name.Contains("WithMissingSsmParameterOnly"))
        {
            result.SsmPaths.Add("/db/mysql/main");
            result.SsmPaths.Add("/message-broker/kafka/hermes");
        }

        if (name == "profile2")
        {
            result.SsmPaths.Add("/message-broker/kafka/cdc");
        }

        if (name.Contains("MissingSsmParameter"))
        {
            result.SsmPaths.Add("/missing/test");
        }
        
        return result;
    }

    public void Save(string name, ProfileDo data)
    {
    }

    public void Delete(string name)
    {
    }
}