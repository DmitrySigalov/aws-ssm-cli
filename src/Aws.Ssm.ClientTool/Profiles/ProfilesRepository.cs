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
            "Default",
            "Profile1",
            "Profile2",
            // "Profile3WithAddedMissingSsmParameter",
            // "Profile4WithMissingSsmParameterOnly",
            // "UnavailableProfile",
        };
    }

    public ProfileDo GetByName(string name)
    {
        if (name == "UnavailableProfile") return null;
        
        var result = new ProfileDo();

        result.EnvironmentVariablePrefix = "";
        result.EnvironmentVariableDelimeter = '_';
        result.EnvironmentVariableNamingConvertType = ProfileDo.NamingConvertTypeEnum.UpperCase;
        
        if (name.Contains( "Profile1"))
        {
            result.EnvironmentVariablePrefix = "SSM_";
            result.EnvironmentVariableDelimeter = '_';
        }

        if (name.Contains( "Profile2"))
        {
            result.EnvironmentVariablePrefix = "SSM-";
            result.EnvironmentVariableDelimeter = '-';
        }

        if (name.Contains( "Profile3"))
        {
            result.EnvironmentVariablePrefix = "ssm_";
            result.EnvironmentVariableNamingConvertType = ProfileDo.NamingConvertTypeEnum.LowerCase;
        }

        if (!name.Contains("WithMissingSsmParameterOnly"))
        {
            result.SsmPaths.Add("/db/mysql/main");
            result.SsmPaths.Add("/message-broker/kafka/hermes");
        }
        
        if (name == "Profile2") result.SsmPaths.Add("/message-broker/kafka/cdc");
        
        if (name.Contains("MissingSsmParameter")) result.SsmPaths.Add("/missing/test");
        
        return result;
    }

    public void Save(string name, ProfileDo data)
    {
    }

    public void Delete(string name)
    {
    }
}