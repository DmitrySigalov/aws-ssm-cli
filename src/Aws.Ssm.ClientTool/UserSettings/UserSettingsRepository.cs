namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsRepository
{
    public UserSettingsDo Load()
    {
        var result = new UserSettingsDo();

        result.Paths.Add("/db/error");
        result.Paths.Add("/db/mysql");
        result.Paths.Add("/message-broker/kafka");
        
        return result;
    }

    public void Save(UserSettingsDo data)
    {
        
    }
}