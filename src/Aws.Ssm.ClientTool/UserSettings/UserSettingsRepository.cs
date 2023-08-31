namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsRepository
{
    public UserSettingsDo Load()
    {
        var result = new UserSettingsDo();

        result.Paths.Add("/db/mysql/main");
        result.Paths.Add("/db/mysql/main-replica-stm");
        result.Paths.Add("/message-broker/kafka/hermes");
        
        return result;
    }

    public void Save(UserSettingsDo data)
    {
        
    }
}