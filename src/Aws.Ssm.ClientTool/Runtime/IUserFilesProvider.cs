namespace Aws.Ssm.ClientTool.Runtime;

public interface IUserFilesProvider
{
    IEnumerable<string> GetFileNames(string fileMask);

    string ReadFileIfExist(string name);
    
    void WriteFileWithBackup(string name, string text);
    
    void DeleteFileWithBackup(string name);
}