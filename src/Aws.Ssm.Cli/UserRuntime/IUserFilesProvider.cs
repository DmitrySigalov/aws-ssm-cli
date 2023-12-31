namespace Aws.Ssm.Cli.UserRuntime;

public interface IUserFilesProvider
{
    string GetFullFilePath(string fileName, FolderTypeEnum folderType);
    
    IEnumerable<string> GetFileNames(string searchPattern, FolderTypeEnum folderType);

    string ReadTextFileIfExist(string name, FolderTypeEnum folderType);
    
    void WriteTextFile(string name, string text, FolderTypeEnum folderType);
    
    void DeleteFile(string name, FolderTypeEnum folderType);
}