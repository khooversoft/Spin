using System.Globalization;
using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Store;

public static partial class PartitionSchemas
{
    // Example: {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
    // Example: {key}/yyyyMM/{key}-yyyyMMdd-HH.{typeName}.json
    // Example: {key}/yyyyMM/{key}-yyyyMMdd-HHmmss.{typeName}.json
    // Example: {key}/{key}-{sequenceNumber}.{typeName}.json
    // 
    public static DateTime ExtractTimeIndex(string path)
    {
        path.NotEmpty();

        Match match = ExtractDateString().Match(path);
        if (match.Success) return parseExact(match.Groups[1].Value);

        match = ExtractDateWithHourString().Match(path);
        if (match.Success) return parseExactHour(match.Groups[1].Value);

        match = ExtractDateWithSecondsString().Match(path);
        if (match.Success) return parseExactSeconds(match.Groups[1].Value);

        match = ExtractSequenceNumberString().Match(path);
        if (match.Success) return parseUnixMilliseconds(match.Groups[1].Value);

        throw new ArgumentException($"Invalid format path={path}", nameof(path));

        static DateTime parseExact(string timeValue) =>
            DateTime.ParseExact(timeValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);

        static DateTime parseExactHour(string timeValue) =>
            DateTime.ParseExact(timeValue, "yyyyMMdd-HH", CultureInfo.InvariantCulture, DateTimeStyles.None);

        static DateTime parseExactSeconds(string timeValue) =>
            DateTime.ParseExact(timeValue, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None);

        static DateTime parseUnixMilliseconds(string timeValue) =>
            DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(timeValue, CultureInfo.InvariantCulture)).UtcDateTime;
    }

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateString();

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8}-\d{2})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateWithHourString();

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8}-\d{6})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateWithSecondsString();

    [GeneratedRegex(@"^[\w.-]+\/[\w.-]+-(\d{15})-\d{6}-[A-Fa-f0-9]+\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractSequenceNumberString();
}
