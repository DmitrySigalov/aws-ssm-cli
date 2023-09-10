namespace Aws.Ssm.Cli.Profiles;

public interface IProfileConfigProvider
{
    string ActiveName { get; set; }

    ISet<string> GetNames();

    ProfileConfig GetByName(string name);

    void Save(string name, ProfileConfig data);

    void Delete(string name);
}