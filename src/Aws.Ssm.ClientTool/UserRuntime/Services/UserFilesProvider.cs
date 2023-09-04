using System.Reflection;

namespace Aws.Ssm.ClientTool.UserRuntime.Services;

public class UserFilesProvider : IUserFilesProvider
{
    private readonly UserParameters _userParameters;

    public UserFilesProvider(UserParameters userParameters)
    {
        _userParameters = userParameters;
    }

    public IEnumerable<string> GetFileNames(string searchPattern)
    {
        var rootFolderPath = GetUserRootFolder();

        return Directory
            .GetFiles(rootFolderPath, searchPattern)
            .Select(x => new FileInfo(x))
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToHashSet();
    }

    public string ReadTextFileIfExist(string name)
    {
        var fullFilePath = GetFullFilePath(name);

        if (!File.Exists(fullFilePath))
        {
            return null;
        }

        using var fileStream = File.OpenText(fullFilePath);
        
        return fileStream.ReadToEnd();
    }

    public void WriteTextFile(string name, string text)
    {
        var fullFilePath = GetFullFilePath(name);

        MoveFileToBackupIfExists(fullFilePath);

        using var fileStream = File.CreateText(fullFilePath);
        
        fileStream.Write(text);
        
        fileStream.Flush();
    }

    public void DeleteFile(string name)
    {
        var fullFilePath = GetFullFilePath(name);

        MoveFileToBackupIfExists(fullFilePath);
    }

    private void MoveFileToBackupIfExists(string fullFilePath)
    {
        var backupFullFilePath = fullFilePath + "backup";

        if (File.Exists(backupFullFilePath))
        {
            File.Delete(backupFullFilePath);
        }
        
        if (File.Exists(fullFilePath))
        {
            File.Move(fullFilePath, backupFullFilePath);
        }
    }
    
    private string GetFullFilePath(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(fileName);
        }
        
        var rootFolderPath = GetUserRootFolder();

        return Path.Combine(rootFolderPath, fileName);
    }
    
    private string GetUserRootFolder()
    {
        var rootPath = Assembly.GetExecutingAssembly().Location;

        if (_userParameters.IsDebug)
        {
            rootPath = Directory.GetCurrentDirectory();
        }

        return Path.GetFullPath(rootPath);
    }
}