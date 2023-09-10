namespace Aws.Ssm.Cli.UserRuntime;

public interface IUserFilesProvider
{
    string GetFullFilePath(string fileName, UserFileLevelEnum level);
    
    IEnumerable<string> GetFileNames(string searchPattern, UserFileLevelEnum level);

    string ReadTextFileIfExist(string name, UserFileLevelEnum level);
    
    void WriteTextFile(string name, string text, UserFileLevelEnum level);
    
    void DeleteFile(string name, UserFileLevelEnum level);
}