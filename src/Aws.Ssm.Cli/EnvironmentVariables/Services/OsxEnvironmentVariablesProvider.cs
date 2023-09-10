using Aws.Ssm.Cli.EnvironmentVariables.Rules;
using Aws.Ssm.Cli.Helpers;
using Aws.Ssm.Cli.UserRuntime;
using Microsoft.Extensions.Logging;

namespace Aws.Ssm.Cli.EnvironmentVariables.Services;

public class OsxEnvironmentVariablesProvider : IEnvironmentVariablesProvider
{
    private readonly IUserFilesProvider _userFilesProvider;

    private readonly ILogger<OsxEnvironmentVariablesProvider> _logger;

    private SortedDictionary<string, string> _loadedDescriptor = null;

    public OsxEnvironmentVariablesProvider(
        IUserFilesProvider userFilesProvider,
        ILogger<OsxEnvironmentVariablesProvider> logger)
    {
        _userFilesProvider = userFilesProvider;

        _logger = logger;
    }

    public ISet<string> GetNames(string baseName = null)
    {
        baseName = baseName?.Trim();
        
        var result = Environment
            .GetEnvironmentVariables()
            .Keys
            .Cast<string>()
            .ToHashSet();

        if (!string.IsNullOrEmpty(baseName))
        {
            result.ExceptWith(result.Where(x => !x.StartsWith(baseName, StringComparison.InvariantCulture)));
        }

        return result;
    }

    public string Get(string name)
    {
        return Environment.GetEnvironmentVariable(name);
    }

    public void Set(string name, string value)
    {
        var environmentVariables = LoadEnvironmentVariablesFromDescriptor();

        if (value == null)
        {
            environmentVariables.Remove(name);
        }
        else
        {
            environmentVariables[name] = value;
        }
        
        DumpEnvironmentVariables(environmentVariables);
        
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    private SortedDictionary<string, string> LoadEnvironmentVariablesFromDescriptor()
    {
        if (_loadedDescriptor != null)
        {
            return _loadedDescriptor;
        }
        
        var fileDescriptorName = EnvironmentVariablesConsts.FileNames.Descriptor;

        try
        {
            var fileDescriptorText = _userFilesProvider.ReadTextFileIfExist(fileDescriptorName);

            if (!string.IsNullOrEmpty(fileDescriptorText))
            {
                _loadedDescriptor = JsonSerializationHelper.Deserialize<SortedDictionary<string, string>>(fileDescriptorText);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to read descriptor file with list of active environment variables");
        }

        return _loadedDescriptor ?? new SortedDictionary<string, string>();
    }

    private void DumpEnvironmentVariables(SortedDictionary<string, string> environmentVariables)
    {
        var fileDescriptorName = EnvironmentVariablesConsts.FileNames.Descriptor;
        var fileScriptName = EnvironmentVariablesConsts.FileNames.Script;

        try
        {
            var fileDescriptorText = JsonSerializationHelper.Serialize(environmentVariables);
            _userFilesProvider.WriteTextFile(fileDescriptorName, fileDescriptorText);

            var fileScriptText = EnvironmentVariablesScriptTextBuilder.Build(environmentVariables);
            _userFilesProvider.WriteTextFile(fileScriptName, fileScriptText);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "Error on attempt to dump descriptor/script file(s) with list of active environment variables");
        }
    }
}