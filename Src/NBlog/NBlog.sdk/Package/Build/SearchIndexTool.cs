using System.Diagnostics;
using Toolbox.DocumentSearch;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class SearchIndexTool
{
    public static async Task<BuildContext> BuildSearchIndex(BuildContext buildContext, ScopeContext context)
    {
        context.LogInformation("Building search index");

        WordTokenList wordList = await WordTokenFiles.Search(buildContext.BasePath);
        var tokenizer = new DocumentTokenizer(wordList);

        var files = buildContext.ManifestFiles
            .SelectMany(x => filterOnAttribute(x.Commands), (o, i) => (man: o.Manifest, file: i))
            .ToArray();

        var documentReferences = await ScrapFiles(tokenizer, files);
        buildContext = await BuildIndexes(buildContext, tokenizer, documentReferences, context);

        context.LogInformation(
            "Completed building search index, total manifest={manifestCount}, file processed={fileProcessedCount}",
            buildContext.ManifestFiles.Count,
            files.Length
            );

        return buildContext;

        IEnumerable<string> filterOnAttribute(IEnumerable<CommandNode> list) => list
            .Where(x => x.Attributes.Any(y => NBlogConstants.FileAttributes.Contains(y)))
            .Select(x => x.LocalFilePath);
    }

    private static async Task<IReadOnlyList<DocumentReference>> ScrapFiles(DocumentTokenizer tokenizer, IReadOnlyList<(ArticleManifest manifest, string file)> list)
    {
        var result = await ActionParallel.RunAsync(list, async x =>
        {
            string line = await File.ReadAllTextAsync(x.file);
            var tokens = tokenizer.Parse(line);

            string dbName = TagsTool.TryGetValue(x.manifest.Tags, NBlogConstants.DbTag, out var v1) ? v1.NotEmpty() : throw new UnreachableException();
            var tags = new Tags(x.manifest.Tags);
            return new DocumentReference(dbName, x.manifest.ArticleId, tokens, tags.Keys).ToOption();
        });

        var docReference = result.Select(x => x.Return()).ToArray();
        return docReference;
    }

    private static async Task<BuildContext> BuildIndexes(BuildContext buildContext, DocumentTokenizer tokenizer, IReadOnlyList<DocumentReference> documentReferences, ScopeContext context)
    {
        var list = documentReferences
            .GroupBy(x => x.DbName, StringComparer.OrdinalIgnoreCase)
            .Select(x => (dbName: x.Key, docRefs: x.ToArray()))
            .ToArray();

        foreach (var item in list)
        {
            buildContext = await BuildIndex(buildContext, item.dbName, item.docRefs, tokenizer, context);
        }

        return buildContext;
    }

    private static async Task<BuildContext> BuildIndex(BuildContext buildContext, string dbName, DocumentReference[] docRefs, DocumentTokenizer tokenizer, ScopeContext context)
    {
        context.Location().LogInformation("Building search index for db={dbName}", dbName);

        var docGroup = docRefs
            .GroupBy(x => x.DocumentId, StringComparer.OrdinalIgnoreCase)
            .Select(doc =>
            {
                var words = doc
                    .SelectMany(x => x.Words)
                    .GroupBy(x => x.Word, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new WordToken(x.Key, x.Max(y => y.Weight)))
                    .ToArray();

                var tags = doc
                    .SelectMany(x => x.Tags)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                return new DocumentReference("db", doc.Key, words, tags);
            })
            .ToArray();

        var documentIndex = new DocumentIndexBuilder()
            .SetTokenizer(tokenizer)
            .Add(docGroup)
            .Build();

        string json = documentIndex.ToSerialization().ToJson();
        string zipFile = NBlogConstants.Tool.CreateSearchIndexActorKey(dbName);

        string sourceFile = buildContext.CreateWorkFile(zipFile);
        await File.WriteAllTextAsync(sourceFile, json);

        buildContext = buildContext with
        {
            CopyCommands = buildContext.CopyCommands + (sourceFile, zipFile),
        };

        return buildContext;
    }
}
