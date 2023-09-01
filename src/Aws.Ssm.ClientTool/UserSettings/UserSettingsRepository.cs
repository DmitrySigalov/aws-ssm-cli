namespace Aws.Ssm.ClientTool.UserSettings;

public class UserSettingsRepository
{
    public UserSettingsDo Get()
    {
        Console.WriteLine("Stub");

        var result = new UserSettingsDo();

        result.SsmPaths.Add("/db/mysql");
        result.SsmPaths.Add("/message-broker/kafka");
        result.SsmPaths.Add("/message-broker/error");
        
        return result;
    }

    public void Save(UserSettingsDo data)
    {
        Console.WriteLine("Not implemented");
    }
}