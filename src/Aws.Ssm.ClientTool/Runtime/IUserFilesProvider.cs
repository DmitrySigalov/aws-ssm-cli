namespace Aws.Ssm.ClientTool.Runtime;

public interface IUserFilesProvider
{
    IEnumerable<string> GetFileNames(string searchPattern);

    string ReadTextFileIfExist(string name);
    
    void WriteTextFile(string name, string text);
    
    void DeleteFile(string name);
}