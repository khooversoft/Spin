using Microsoft.Extensions.Logging;
using Toolbox.DocumentSearch;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class BuildWordTokenList
{
    private readonly ILogger<BuildWordTokenList> _logger;
    public BuildWordTokenList(ILogger<BuildWordTokenList> logger) => _logger = logger.NotNull();

    public async Task<Option> Build(string basePath, string wordTokenListFile)
    {
        basePath.NotEmpty();
        wordTokenListFile.NotEmpty();
        var context = new ScopeContext(_logger);

        context.LogInformation("Building word token list, basePath={basePath}, wordTokenListFile={wordTokenListFile}", basePath, wordTokenListFile);
        if (File.Exists(wordTokenListFile)) File.Delete(wordTokenListFile);

        var manifestFileListOption = await ManifestFileList.Build(basePath, context);
        if (manifestFileListOption.IsError()) return manifestFileListOption.ToOptionStatus();
        ManifestFileList manifestFileList = manifestFileListOption.Return();

        WordTokenList wordTokens = await WordTokenFiles.Search(basePath);
        WordTokenList result = await BuildList(manifestFileList.Files, wordTokens, context);

        string json = result.ToJson();
        await File.WriteAllTextAsync(wordTokenListFile, json);

        context.LogInformation("Completed: word list tokens writen to file={file}, files added={count}", wordTokenListFile, result.Count);
        return StatusCode.OK;
    }

    private static async Task<WordTokenList> BuildList(IReadOnlyList<ManifestFile> manifestFiles, WordTokenList wordTokens, ScopeContext context)
    {
        context.LogInformation("Building word token list from documents");
        var tokenizer = new DocumentTokenizer();

        var files = manifestFiles
            .SelectMany(x => x.Commands.Select(y => y.LocalFilePath))
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Key)
            .ToArray();

        var list = new Sequence<WordToken>(wordTokens);

        foreach (var file in files)
        {
            context.LogInformation("Reading document file={file}", file);

            string line = await File.ReadAllTextAsync(file);
            var tokens = tokenizer.Parse(line);
            list += tokens;
        }

        var wordTokenList = new WordTokenList(list);
        context.LogInformation("Document processed count={count}", wordTokenList.Dictionary.Count);
        return wordTokenList;
    }
}
