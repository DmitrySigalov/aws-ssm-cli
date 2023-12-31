using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Aws.Ssm.Cli.UserRuntime.Services;

public class UserFilesProvider : IUserFilesProvider
{
    private readonly IConfiguration _configuration;
    
    public UserFilesProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GetFullFilePath(string fileName, FolderTypeEnum folderType)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            throw new ArgumentNullException(fileName);
        }
        
        var rootFolderPath = GetFolderPath(folderType);

        return Path.Combine(rootFolderPath, fileName);
    }
    
    public IEnumerable<string> GetFileNames(string searchPattern, FolderTypeEnum folderType)
    {
        var rootFolderPath = GetFolderPath(folderType);
        
        return Directory
            .GetFiles(rootFolderPath, searchPattern)
            .Select(x => new FileInfo(x))
            .Select(x => x.Name)
            .OrderBy(x => x)
            .ToHashSet();
    }

    public string ReadTextFileIfExist(string name, FolderTypeEnum folderType)
    {
        var fullFilePath = GetFullFilePath(name, folderType);

        if (!File.Exists(fullFilePath))
        {
            return null;
        }

        using var fileStream = File.OpenText(fullFilePath);
        
        return fileStream.ReadToEnd();
    }

    public void WriteTextFile(string name, string text, FolderTypeEnum folderType)
    {
        var fullFilePath = GetFullFilePath(name, folderType);

        MoveFileToBackupIfExists(fullFilePath);

        using var fileStream = File.CreateText(fullFilePath);
        
        fileStream.Write(text);
        
        fileStream.Flush();
    }

    public void DeleteFile(string name, FolderTypeEnum folderType)
    {
        var fullFilePath = GetFullFilePath(name, folderType);

        MoveFileToBackupIfExists(fullFilePath);
    }

    private void MoveFileToBackupIfExists(string fullFilePath)
    {
        var backupFullFilePath = fullFilePath + ".backup";

        if (File.Exists(backupFullFilePath))
        {
            File.Delete(backupFullFilePath);
        }
        
        if (File.Exists(fullFilePath))
        {
            File.Move(fullFilePath, backupFullFilePath);
        }
    }
    
    private string GetFolderPath(FolderTypeEnum folderType)
    {
        var path = _configuration.GetValue<string>("USER_FOLDER_PATH");
        
        if (string.IsNullOrWhiteSpace(path))
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = $"C:/Users/{_configuration["USERNAME"]}";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = $"/Users/{Environment.UserName}";
            }
            else
            {
                throw new NotSupportedException("Not supported for operation system");
            }
        }
        
        path = Path.GetFullPath(path);

        if (folderType == FolderTypeEnum.UserConfiguration)
        {
            path = Path.Combine(path, ".aws-ssm-cli");
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        return path;
    }
}