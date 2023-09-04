using System.Reflection;

namespace Aws.Ssm.ClientTool.Runtime.Services;

public class UserFilesProvider : IUserFilesProvider
{
    private readonly RuntimeParameters _runtimeParameters;

    public UserFilesProvider(RuntimeParameters runtimeParameters)
    {
        _runtimeParameters = runtimeParameters;
    }

    public IEnumerable<string> GetFileNames(string fileMask)
    {
        throw new NotImplementedException();
    }

    public string ReadFileIfExist(string name)
    {
        throw new NotImplementedException();
    }

    public void WriteFileWithBackup(string name, string text)
    {
        throw new NotImplementedException();
    }

    public void DeleteFileWithBackup(string name)
    {
        throw new NotImplementedException();
    }
    
    private string GetUserRootFolder()
    {
        if (_runtimeParameters.IsDebug)
        {
            return Directory.GetCurrentDirectory();
        }

        return Assembly.GetExecutingAssembly().Location;
    }
}