using System.Globalization;
using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Store;

public static partial class PartitionSchemas
{
    // Example: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
    // Example: {key}/yyyyMM/{key}-yyyyMMdd-HHmmss.{typeName}.json
    // Extract yyyyMMdd from the path
    public static DateTime ExtractTimeIndex(string path)
    {
        path.NotEmpty();

        DateTime date = ExtractDateString().Match(path) switch
        {
            { Success: true } v => parseExact(v.Groups[1].Value),
            _ => ExtractDateWithSecondsString().Match(path) switch
            {
                { Success: true } v2 => parseExactSeconds(v2.Groups[1].Value),
                _ => throw new ArgumentException("Invalid format path={path}", path),
            }
        };

        return date;

        static DateTime parseExact(string timeValue) =>
            DateTime.ParseExact(timeValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);

        static DateTime parseExactSeconds(string timeValue) =>
            DateTime.ParseExact(timeValue, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);
    }

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateString();

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8}-\d{6})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateWithSecondsString();
}
