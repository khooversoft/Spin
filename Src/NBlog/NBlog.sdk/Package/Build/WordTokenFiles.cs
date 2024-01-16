using System.Collections.Concurrent;
using Toolbox.DocumentSearch;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class WordTokenFiles
{
    public static async Task<Option<WordTokenList>> ReadFile(string file)
    {
        if (!File.Exists(file)) return StatusCode.NotFound;

        string json = await File.ReadAllTextAsync(file);
        if (json.IsEmpty()) return StatusCode.NoContent;

        var list = new WordTokenListBuilder().SetJson(json).Build();
        if (list == null) return (StatusCode.Conflict, "Failed to parse");

        return list;
    }

    public static async Task<WordTokenList> Search(string basePath, string search = "*" + NBlogConstants.WordTokenExtension)
    {
        string[] files = Directory.GetFiles(basePath, search, SearchOption.AllDirectories)
            .Where(x => x.IndexOf(".vscode") < 0)
            .ToArray();

        var queue = new ConcurrentQueue<WordTokenList>();
        await ActionParallel.Run<string>(async file =>
        {
            var result = await ReadFile(file);
            if (result.IsOk()) queue.Enqueue(result.Return());
        }, files);

        var mergedList = queue
            .SelectMany(x => x)
            .GroupBy(x => x.Word)
            .Select(x => new WordToken(x.Key, x.Max(y => y.Weight)));

        return new WordTokenList(mergedList);
    }
}
