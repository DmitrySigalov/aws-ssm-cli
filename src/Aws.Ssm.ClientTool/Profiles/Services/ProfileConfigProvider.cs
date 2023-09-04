using Aws.Ssm.ClientTool.Profiles.Rules;
using Aws.Ssm.ClientTool.Runtime;

namespace Aws.Ssm.ClientTool.Profiles.Services;

public class ProfileConfigProvider : IProfileConfigProvider
{
    private readonly IUserFilesProvider _userFilesProvider;

    public ProfileConfigProvider(IUserFilesProvider userFilesProvider)
    {
        _userFilesProvider = userFilesProvider;
    }
    
    public string ActiveName
    {
        get => GetNames().FirstOrDefault();
        set
        {
        }
    }
    
    public ISet<string> GetNames()
    {
        return _userFilesProvider
            .GetFileNames(ProfileFileNameResolver.SearchFileNamePattern)
            .Select(ProfileFileNameResolver.ExtractProfileName)
            .ToHashSet();
    }

    public ProfileConfig GetByName(string name)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        var fileText = _userFilesProvider
            .ReadTextFileIfExist(fileName);

        if (fileText == null)
        {
            throw new ArgumentException($"No found profile file named {fileName}");
        }
        
        if (name == "unavailableProfile") return null;
        
        var result = new ProfileConfig();

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

    public void Save(string name, ProfileConfig data)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        var fileText = $"Dummy #{DateTime.UtcNow}";
        
        _userFilesProvider.WriteTextFile(fileName, fileText);
    }

    public void Delete(string name)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);
        
        _userFilesProvider.DeleteFile(fileName);
    }
}