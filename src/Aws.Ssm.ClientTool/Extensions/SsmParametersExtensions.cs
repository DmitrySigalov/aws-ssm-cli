using Aws.Ssm.ClientTool.Profiles;
using Aws.Ssm.ClientTool.Utils;
using ConsoleTables;

namespace Aws.Ssm.ClientTool.Extensions;

public static class SsmParametersExtensions
{
    public static void PrintSsmParameters(
        this IDictionary<string, string> ssmParameters,
        ProfileDo profileDo)
    {
        if (ssmParameters.Any() == false)
        {
            ConsoleUtils.WriteLineWarn("Ssm parameters empty list");
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
            ConsoleUtils.Warn(() =>
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