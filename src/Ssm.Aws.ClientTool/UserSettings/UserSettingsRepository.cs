namespace Ssm.Aws.ClientTool.UserSettings;

public class UserSettingsRepository
{
    public UserSettingsDo Load()
    {
        var result = new UserSettingsDo();

        result.Paths.Add("/db/mysql/main");
        result.Paths.Add("/db/mysql/main-replica-stm");
        result.Paths.Add("/message-broker/kafka/hermes");
        
        return new UserSettingsDo();
    }

    public void Save(UserSettingsDo data)
    {
        
    }
}