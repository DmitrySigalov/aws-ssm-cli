using Kurukuru;

namespace Aws.Ssm.ClientTool.Utils;

public static class SpinnerUtils
{
    public static TResult Run<TResult>(
        Func<TResult> callback,
        string text)
    {
        using var spinner = StartSpinner(text);

        return Run(spinner, callback);
    }

    private static Spinner StartSpinner(string text)
    {
        var spinner = new Spinner(text);
        spinner.SymbolSucceed = new SymbolDefinition("V", "E");
        
        spinner.Start();

        return spinner;
    }

    private static TResult Run<TResult>(
        this Spinner spinner, 
        Func<TResult> callback)
    {
        try
        {
            var result = callback();

            spinner.Succeed();

            return result;
        }
        catch
        {
            spinner.Fail();

            throw;
        }
    }
}