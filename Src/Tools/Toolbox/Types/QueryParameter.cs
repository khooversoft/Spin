using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;

/// <summary>
/// Query string
///     "{filter}"
///     "{filter};index={index};count={count};recurse={tf};includeFile={tf};includeFolder={tf}"
///     "filter={filter};index={index};count={count};recurse={tf};includeFile={tf};includeFolder={tf}"
///
///   Filter =  "*" - matches anything
///             "path" - matches 'path'
///             "path/*" - matches anything in 'path' folder
///             "path/*.ext" - matches all the files in 'path' folder that ends with ".ext"
///             "path/**/*" - matches all files in all folders under 'path'
///             "path/**/*.ext" - matches all files in all folders under 'path' that ends with ".ext"
/// </summary>
public record QueryParameter
{
    public int Index { get; init; } = 0;
    public int Count { get; init; } = 1000;
    public string Filter { get; init; } = null!;
    public bool Recurse { get; init; }
    public bool IncludeFile { get; init; } = true;
    public bool IncludeFolder { get; init; }
    public string BasePath { get; init; } = null!;


    public static QueryParameter Parse(string value)
    {
        Stack<KeyValuePair<string, string?>> values = PropertyStringSchema.FileSearch.Parse(value)
            .ThrowOnError().Return()
            .Reverse()
            .ToStack();

        int index = 0;
        int count = 1000;
        string filter = null!;
        bool recurse = false;
        bool includeFile = true;
        bool includeFolder = false;

        while (values.TryPop(out var entry))
        {
            switch (entry.Key.ToLower())
            {
                case "filter": filter = entry.Value.NotEmpty(); break;
                case "index": index = int.TryParse(entry.Value, out int indexResult) ? indexResult : 0; break;
                case "count": count = int.TryParse(entry.Value, out int countResult) ? countResult : 0; break;
                case "recurse": recurse = bool.TryParse(entry.Value, out bool recursiveResult) ? recursiveResult : false; break;
                case "includefile": includeFile = bool.TryParse(entry.Value, out bool includeFileResult) ? includeFileResult : false; break;
                case "includefolder": includeFolder = bool.TryParse(entry.Value, out bool includeFolderResult) ? includeFolderResult : false; break;

                default:
                    entry.Value.Assert(x => x.IsEmpty(), $"Unknown property, key={entry.Key}");
                    filter = entry.Key.NotEmpty();
                    break;
            }
        }

        filter = filter.ToNullIfEmpty() ?? "*";
        recurse = recurse || filter.Contains("**");

        string basePath = filter
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .TakeWhile(x => x.IndexOf('*') < 0)
            .Join('/');

        var queryParameter = new QueryParameter
        {
            Index = index,
            Count = count,
            Filter = filter,
            Recurse = recurse,
            IncludeFile = includeFile,
            IncludeFolder = includeFolder,
            BasePath = basePath,
        };

        return queryParameter;
    }
}

public class QueryParameterMatcher
{
    private readonly QueryParameter _queryParameter;
    private readonly GlobFileMatching _matcher;

    public QueryParameterMatcher(QueryParameter queryParameter)
    {
        _queryParameter = queryParameter.NotNull();
        _matcher = new GlobFileMatching(queryParameter.Filter);
    }

    public bool IsMatch(string path, bool isFolder)
    {
        path.NotEmpty();

        var include = (_queryParameter.IncludeFile, _queryParameter.IncludeFolder, isFolder) switch
        {
            (false, false, _) => true,
            (true, true, _) => true,

            (true, false, false) => true,
            (true, false, true) => false,

            (false, true, false) => false,
            (false, true, true) => true,
        };

        if (!include) return false;

        var match = _matcher.IsMatch(path);
        return match;
    }
}

public static class QueryParameterExtensions
{
    public static string ToQueryString(this QueryParameter subject)
    {
        subject.NotNull();

        return new string?[]
        {
            subject.Filter?.ToString()?.Func(x => $"filter={Uri.EscapeDataString(x)}"),
            $"index={subject.Index}",
            $"count={subject.Count}",
            $"recurse={subject.Recurse}",
            $"includeFile={subject.IncludeFile}",
            $"includeFolder={subject.IncludeFolder}",
        }
        .Where(x => x != null)
        .Join('&');
    }

    public static QueryParameterMatcher GetMatcher(this QueryParameter subject) => new QueryParameterMatcher(subject);

    //public static bool IsMatch(this QueryParameter subject, string path, bool isFolder)
    //{
    //    subject.NotNull();
    //    path.NotEmpty();

    //    var include = (subject.IncludeFile, subject.IncludeFolder, isFolder) switch
    //    {
    //        (false, false, _) => true,
    //        (true, true, _) => true,

    //        (true, false, false) => true,
    //        (true, false, true) => false,

    //        (false, true, false) => false,
    //        (false, true, true) => true,
    //    };

    //    if (!include) return false;

    //    var match = path.Match(subject.Filter);
    //    return match;
    //}
}
