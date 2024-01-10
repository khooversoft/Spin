using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Types;

namespace NBlog.sdk.test.Directory;

public class ArticleDirectoryQueryTests
{
    private static readonly ScopeContext _context = new ScopeContext(NullLogger.Instance);

    [Fact]
    public void LoadGraphMapFromData()
    {
        var directoryApi = DirectoryData.Load();
        directoryApi.Should().NotBeNull();

        ((DirectoryFake)directoryApi).Map.Nodes.Count.Should().Be(29);
        ((DirectoryFake)directoryApi).Map.Edges.Count.Should().Be(19);
    }

    [Fact]
    public void QueryTest()
    {
        var rawData = "select ('db=article') nodes -> [summary] xref -> (key=file:*);";
        var result = GraphLang.Parse(rawData);
        result.IsOk().Should().BeTrue();

        rawData = "select (tags='db=article') nodes -> [summary] xref -> (key=file:*);";
        result = GraphLang.Parse(rawData);
        result.IsOk().Should().BeTrue();
    }

    [Fact]
    public async Task GetSummariesForDbArticle()
    {
        var directoryApi = DirectoryData.Load();
        directoryApi.Should().NotBeNull();

        var api = new ArticleDirectoryClient(directoryApi, NullLogger<ArticleDirectoryClient>.Instance);

        IReadOnlyList<ArticleFileReference> data = await api.GetSummaries("article", _context);
        data.Should().NotBeNull();
        data = data.OrderBy(x => x.ArticleId).ToArray();

        var matchTo = DirectoryData.TestArticleManifests
            .Where(x => TagsTool.HasTag(x.Tags, "db", "article") && !TagsTool.HasTag(x.Tags, "noSummary"))
            .Select(x => new ArticleFileReference
            {
                ArticleId = x.ArticleId,
                CreatedDate = x.CreatedDate,
                FileId = x.GetCommands().Where(y => y.Attributes.Contains("summary")).FirstOrDefault()?.FileId!,
            })
            .Where(x => x.FileId != null)
            .OrderBy(x => x.ArticleId)
            .ToArray();

        data.Count.Should().Be(matchTo.Length);
        Enumerable.SequenceEqual(data, matchTo).Should().BeTrue();
    }

    [Fact]
    public async Task GetSummariesForDbResume()
    {
        var directoryApi = DirectoryData.Load();
        directoryApi.Should().NotBeNull();

        var api = new ArticleDirectoryClient(directoryApi, NullLogger<ArticleDirectoryClient>.Instance);

        IReadOnlyList<ArticleFileReference> data = await api.GetSummaries("resume", _context);
        data.Should().NotBeNull();
        data = data.OrderBy(x => x.ArticleId).ToArray();

        var matchTo = DirectoryData.TestArticleManifests
            .Where(x => TagsTool.HasTag(x.Tags, "db", "resume") && !TagsTool.HasTag(x.Tags, "noSummary"))
            .Select(x => new ArticleFileReference
            {
                ArticleId = x.ArticleId,
                CreatedDate = x.CreatedDate,
                FileId = x.GetCommands().Where(y => y.Attributes.Contains("summary")).FirstOrDefault()?.FileId!,
            })
            .Where(x => x.FileId != null)
            .OrderBy(x => x.ArticleId)
            .ToArray();

        data.Count.Should().Be(matchTo.Length);
        Enumerable.SequenceEqual(data, matchTo).Should().BeTrue();
    }

    [Fact]
    public async Task GetIndexes()
    {
        var directoryApi = DirectoryData.Load();
        directoryApi.Should().NotBeNull();

        var api = new ArticleDirectoryClient(directoryApi, NullLogger<ArticleDirectoryClient>.Instance);

        IReadOnlyList<ArticleIndex> data = await api.GetSummaryIndexes("resume", _context);
        data.Should().NotBeNull();
        data = data.OrderBy(x => x.ArticleId).ThenBy(x => x.IndexName).ThenBy(x => x.FileId).ToArray();

        var matchTo = DirectoryData.TestArticleManifests
            .Where(x => TagsTool.HasTag(x.Tags, "db", "resume") && !TagsTool.HasTag(x.Tags, "noSummary"))
            .SelectMany(x => Tags.Create(x.Tags)
                .Where(x => x.Value.IsNotEmpty())
                .Where(x => !x.Key.EqualsIgnoreCase("db"))
                .OfType<KeyValuePair<string, string>>(),
                (o, i) => (articleId: o.ArticleId, indexName: $"{i.Key}/{i.Value}", commands: o.GetCommands())
            )
            .Select(x => new ArticleIndex
            {
                IndexName = x.indexName,
                ArticleId = x.articleId,
                FileId = x.commands.Where(x => x.Attributes.Contains("summary")).FirstOrDefault()?.FileId!,
            })
            .Where(x => x.FileId != null)
            .OrderBy(x => x.ArticleId).ThenBy(x => x.IndexName).ThenBy(x => x.FileId)
            .ToArray();

        string dataDump = data.ToJson();
        string matchToDump = matchTo.ToJson();

        Enumerable.SequenceEqual(data, matchTo).Should().BeTrue();
    }
}
