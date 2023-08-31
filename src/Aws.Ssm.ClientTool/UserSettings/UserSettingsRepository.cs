namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsRepository
{
    public UserSettingsDo Get()
    {
        var result = new UserSettingsDo();

        result.SsmPaths.Add("/db/mysql");
        result.SsmPaths.Add("/message-broker/kafka");
        
        return result;
    }

    public void Save(UserSettingsDo data)
    {
        
    }
}