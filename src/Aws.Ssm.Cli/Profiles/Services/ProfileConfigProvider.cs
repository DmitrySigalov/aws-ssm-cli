using Aws.Ssm.Cli.EnvironmentVariables;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.Profiles.Rules;
using Aws.Ssm.Cli.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Aws.Ssm.Cli.Profiles.Services;

public class ProfileConfigProvider : IProfileConfigProvider
{
    private readonly IUserFilesProvider _userFilesProvider;

    private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

    private readonly ILogger<ProfileConfigProvider> _logger;

    public ProfileConfigProvider(
        IUserFilesProvider userFilesProvider,
        IEnvironmentVariablesProvider environmentVariablesProvider,
        ILogger<ProfileConfigProvider> logger)
    {
        _userFilesProvider = userFilesProvider;

        _environmentVariablesProvider = environmentVariablesProvider;

        _logger = logger;
    }
    
    public string ActiveName
    {
        get => _environmentVariablesProvider.Get(EnvironmentVariablesConsts.GetClientToolVariableName(nameof(ActiveName)));
        set => _environmentVariablesProvider.Set(EnvironmentVariablesConsts.GetClientToolVariableName(nameof(ActiveName)), value);
    }
    
    public ISet<string> GetNames()
    {
        return _userFilesProvider
            .GetFileNames(ProfileFileNameResolver.SearchFileNamePattern, FolderTypeEnum.UserConfiguration)
            .Select(ProfileFileNameResolver.ExtractProfileName)
            .OrderBy(x => x)
            .ToHashSet();
    }

    public ProfileConfig GetByName(string name)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = _userFilesProvider
                .ReadTextFileIfExist(fileName, FolderTypeEnum.UserConfiguration);

            return JsonSerializationHelper.Deserialize<ProfileConfig>(fileText);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to read profile configuration. One of possible reasons - file was corrupted. Continue with default settings.");

            return null;
        }
    }

    public void Save(string name, ProfileConfig data)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            var fileText = JsonSerializationHelper.Serialize(data);
        
            _userFilesProvider.WriteTextFile(fileName, fileText, FolderTypeEnum.UserConfiguration);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to save profile configuration. Continue with default settings.");
        }
    }

    public void Delete(string name)
    {
        var fileName = ProfileFileNameResolver.BuildFileName(name);

        try
        {
            _userFilesProvider.DeleteFile(fileName, FolderTypeEnum.UserConfiguration);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to delete profile file");
        }
    }
}