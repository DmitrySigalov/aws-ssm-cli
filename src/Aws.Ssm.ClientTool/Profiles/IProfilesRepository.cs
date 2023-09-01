namespace Aws.Ssm.ClientTool.Profiles;

public interface IProfilesRepository
{
    string ActiveName { get; set; }

    ISet<string> GetNames();

    ProfileDo GetByName(string name);

    void Save(string name, ProfileDo data);
}