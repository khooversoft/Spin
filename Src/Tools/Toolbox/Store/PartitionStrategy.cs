using System.Globalization;
using System.Text.RegularExpressions;
using Toolbox.Tools;

namespace Toolbox.Store;


/// File partitioning schemas
///   File = {h1}/{h2}/{key}.{typeName}.json
///   FileSearch = ?"
///   
///   List = {key}/yyyyMM/{key}-yyyyMMdd.{typeName}.json
///   ListSearch = {key}/?"
///   
/// timeIndex = "yyyyMM"
/// Day timeIndex = "yyyyMMdd"
/// 


public static partial class PartitionSchemas
{
    public static string ScalarPath<T>(string key)
    {
        key.NotEmpty();

        var path = $"{key}.{typeof(T).Name}.json";
        var hashPath = PathTool.CreateHashPath(path);

        var result = $"{hashPath}/{path}";
        return result;
    }

    public static string ScalarSearch(string _, string pattern) => $"*/*/{pattern.NotEmpty()}";

    public static string ListPath<T>(string key)
    {
        key.NotEmpty();
        DateTime now = DateTime.UtcNow;

        return ListPath(key, typeof(T).Name, DateTime.UtcNow);
    }

    public static string ListPath(string key, string listType, DateTime date)
    {
        key.NotEmpty();
        listType.NotEmpty();

        var path = $"{key}/{date:yyyyMM}/{key}-{date:yyyyMMdd}.{listType}.json";
        return path;
    }

    public static string ListPathBySeconds(string key, string listType, DateTime date)
    {
        key.NotEmpty();
        listType.NotEmpty();

        var path = $"{key}/{date:yyyyMM}/{key}-{date:yyyyMMdd-HHmmss}.{listType}.json";
        return path;
    }

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

    public static string ListSearch(string key, string? pattern)
    {
        key.NotEmpty();

        var path = pattern switch
        {
            string p => $"{key}/{pattern}",
            _ => key,
        };

        return path;
    }

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateString();

    [GeneratedRegex(@"^[\w.-]+\/\d{6}\/[\w.-]+-(\d{8}-\d{6})\.[\w.-]+\.json$", RegexOptions.CultureInvariant)]
    private static partial Regex ExtractDateWithSecondsString();
}
