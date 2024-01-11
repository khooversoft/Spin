using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.DocumentSearch;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class PackageBuild
{
    private readonly ILogger<PackageBuild> _logger;
    public PackageBuild(ILogger<PackageBuild> logger) => _logger = logger.NotNull();

    public async Task<Option> Build(string basePath, string packageFile)
    {
        packageFile = PathTools.SetExtension(packageFile, NBlogConstants.PackageExtension);
        var context = new ScopeContext(_logger);
        context.LogInformation("Building package, basePath={basePath}, packageFile={packageFile}", basePath, packageFile);
        if (File.Exists(packageFile)) File.Delete(packageFile);

        var manifestFileListOption = await ManifestFileList.Build(basePath, context);
        if (manifestFileListOption.IsError()) return manifestFileListOption.ToOptionStatus();
        ManifestFileList manifestFileList = manifestFileListOption.Return();

        using var zipFile = File.Open(packageFile, FileMode.Create);
        using var zip = new ZipArchive(zipFile, ZipArchiveMode.Create);

        if (WriteConfigurationFile(zip, basePath, context).IsError()) return StatusCode.BadRequest;

        WriteManifestFilesToZip(zip, manifestFileList.Files, context);
        WriteFilesToZip(zip, manifestFileList.Files, context);

        BuildAndWriteIndex(zip, manifestFileList.Files, context);
        await BuildSearchIndex(zip, manifestFileList.Files, basePath, context);

        context.Location().LogInformation("Completed: Package has been created, files added={count}", manifestFileList.Files.Count);
        return StatusCode.OK;
    }

    private Option WriteConfigurationFile(ZipArchive zip, string basePath, ScopeContext context)
    {
        var configFileOption = ConfigurationFile.Read(basePath, context);
        if (configFileOption.IsError()) return configFileOption.ToOptionStatus();

        var configFile = configFileOption.Return();
        string configEntry = PackagePaths.GetConfigurationPath();
        zip.CreateEntryFromFile(configFile, configEntry.ToLower());

        return StatusCode.OK;
    }

    private static void WriteManifestFilesToZip(ZipArchive zip, IReadOnlyList<ManifestFile> manifestFiles, ScopeContext context)
    {
        string tempFile = Path.GetTempFileName();

        try
        {
            foreach (var queueManifest in manifestFiles)
            {
                string json = queueManifest.Manifest.ToJson();
                File.WriteAllText(tempFile, json);

                string manifestFileEntry = PackagePaths.GetManifestZipPath(queueManifest.Manifest.ArticleId);
                zip.CreateEntryFromFile(tempFile, manifestFileEntry.ToLower());
                context.LogInformation("Writing manifest file={file} to zipEntry={zipEntry}", queueManifest.File, manifestFileEntry);
            }

            context.LogInformation("Write manifest count={count} files", manifestFiles.Count);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    private static void WriteFilesToZip(ZipArchive zip, IReadOnlyList<ManifestFile> manifestFiles, ScopeContext context)
    {
        var dataFiles = manifestFiles
            .SelectMany(x => x.Commands)
            .Select(x => (fileId: x.FileId, localFile: x.LocalFilePath))
            .ToArray();

        foreach (var dataFile in dataFiles)
        {
            string fileEntry = PackagePaths.GetDatafileZipPath(dataFile.fileId);
            zip.CreateEntryFromFile(dataFile.localFile, fileEntry.ToLower());
            context.LogInformation("Writing data command file={file} to zipEntry={zipEntry}", dataFile.localFile, fileEntry);
        }

        context.Location().LogInformation("Write data count={count} files", dataFiles.Length);
    }

    private static void BuildAndWriteIndex(ZipArchive zip, IReadOnlyList<ManifestFile> manifestFiles, ScopeContext context)
    {
        Option<GraphMap> index = new ArticleDirectoryBuilder()
            .Add(manifestFiles.Select(x => x.Manifest))
            .Build();

        if (index.IsError())
        {
            context.Location().LogError("Failed to build article directory graph, error={error}", index);
            return;
        }

        string json = index.Return().ToJson();
        WriteJsonToZip(zip, json, PackagePaths.GetPathArticleIndexZipPath(), context);
    }

    private static async Task BuildSearchIndex(ZipArchive zip, IReadOnlyList<ManifestFile> manifestFiles, string basePath, ScopeContext context)
    {
        context.LogInformation("Building search index");

        WordTokenList wordList = await WordTokenFiles.Search(basePath);
        var tokenizer = new DocumentTokenizer(wordList);

        var files = manifestFiles
            .SelectMany(x => x.Commands.Select(y => y.LocalFilePath), (o, i) => (man: o.Manifest, file: i))
            .GroupBy(x => x.file, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var list = new Sequence<DocumentReference>();

        foreach (var file in files)
        {
            string line = await File.ReadAllTextAsync(file.Key);
            var tokens = tokenizer.Parse(line);

            foreach ((ArticleManifest man, string file) article in file)
            {
                var tags = new Tags(article.man.Tags);
                list += new DocumentReference(article.man.ArticleId, tokens, tags.Keys);
            }
        }

        var docGroup = list
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

                return new DocumentReference(doc.Key, words, tags);
            })
            .ToArray();

        var documentIndex = new DocumentIndexBuilder()
            .SetTokenizer(tokenizer)
            .Add(docGroup)
            .Build();

        string json = documentIndex.ToSerialization().ToJson();
        WriteJsonToZip(zip, json, PackagePaths.GetSearchFileZipPath(), context);
    }

    private static void WriteJsonToZip(ZipArchive zip, string json, string zipFile, ScopeContext context)
    {
        json.NotEmpty();
        string tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(tempFile, json);
            zip.CreateEntryFromFile(tempFile, zipFile);

            context.LogInformation("Writing to zipEntry={zipEntry}", zipFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
