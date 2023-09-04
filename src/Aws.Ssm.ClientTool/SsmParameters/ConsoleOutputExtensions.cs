using Aws.Ssm.ClientTool.Helpers;
using Aws.Ssm.ClientTool.Profiles;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.SsmParameters;

public static class ConsoleOutputExtensions
{
    public static void PrintSsmParameters(
        this IDictionary<string, string> ssmParameters,
        ProfileDo profileDo)
    {
        if (ssmParameters.Any() == false)
        {
            ConsoleHelper.WriteLineWarn("Ssm parameters empty list");
        }
        else
        {
            var table = new ConsoleTable("ssm-parameter-name", "value");
            foreach (var ssmParam in ssmParameters.OrderBy(x => x.Key))
            {
                table.AddRow(ssmParam.Key, ssmParam.Value);
            }
            table.Write(Format.Minimal);
        }

        var invalidPaths = profileDo.SsmPaths
            .Distinct()
            .Where(x => ssmParameters.Keys.All(y => !y.StartsWith(x)))
            .ToArray();

        if (invalidPaths.Any() == true)
        {
            ConsoleHelper.Warn(() =>
            {
                var table = new ConsoleTable("missing-ssm-path");
                foreach (var ssmPath in invalidPaths.OrderBy(x => x))
                {
                    table.AddRow(ssmPath);
                }

                table.Write(Format.Minimal);
            });
        }
    }
}