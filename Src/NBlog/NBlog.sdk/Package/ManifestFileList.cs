using System.Collections.Concurrent;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class ManifestFileList
{
    public ManifestFileList() { }
    public ManifestFileList(IEnumerable<ManifestFile> files) => Files = files.NotNull().ToArray();

    public IReadOnlyList<ManifestFile> Files { get; init; } = Array.Empty<ManifestFile>();

    public static async Task<Option<ManifestFileList>> Build(string basePath, ScopeContext context)
    {
        basePath.NotEmpty();
        context.LogInformation("Reading manifest files, basePath={basePath}", basePath);

        var manifestFiles = await ReadAllFiles(basePath, context);

        var verifyOption = Option.Root
            .Bind(() => VerifyArticleIds(manifestFiles, context))
            .Bind(() => VerifyFileReferences(manifestFiles, context));

        if (verifyOption.IsError()) return verifyOption.ToOptionStatus<ManifestFileList>();

        return new ManifestFileList(manifestFiles);
    }

    private static async Task<IReadOnlyList<ManifestFile>> ReadAllFiles(string basePath, ScopeContext context)
    {
        var queue = new ConcurrentQueue<ManifestFile>();

        string[] files = Directory.GetFiles(basePath, "*.manifest.json", SearchOption.AllDirectories)
            .Where(x => x.IndexOf(".vscode") < 0)
            .ToArray();

        context.LogInformation("Reading manifest files, count={count}", files.Length);
        if (files.Length == 0)
        {
            context.Location().LogWarning("No files to read");
            return Array.Empty<ManifestFile>();
        }

        await ActionBlockParallel.Run<string>(async x => await ReadManifest(x, basePath, queue, context), files, 1);

        if (files.Length != queue.Count)
        {
            context.Location().LogError("Failed to read {count} files.", files.Length - queue.Count);
            return Array.Empty<ManifestFile>();
        }

        context.Location().LogInformation("Read {count} files", queue.Count);
        return queue.ToArray();
    }

    private static async Task ReadManifest(string file, string basePath, ConcurrentQueue<ManifestFile> queue, ScopeContext context)
    {
        file.NotEmpty();
        basePath.NotEmpty();
        context.LogInformation("Processing file={file}, basePath={basePath}", file, basePath);

        var manifestFile = await ManifestFile.Read(file, basePath, context);
        if (manifestFile.IsError()) return;

        queue.Enqueue(manifestFile.Return());
    }

    private static Option VerifyArticleIds(IReadOnlyList<ManifestFile> manifestFiles, ScopeContext context)
    {
        var fileIds = manifestFiles.Select(x => x.Manifest.ArticleId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (fileIds.Length != manifestFiles.Count)
        {
            string msg = fileIds.Aggregate("Duplicate 'Article Ids'" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.Location().LogError(msg);
            return StatusCode.Conflict;
        }

        return StatusCode.OK;
    }

    private static Option VerifyFileReferences(IReadOnlyList<ManifestFile> manifestFiles, ScopeContext context)
    {
        var multipleReferences = manifestFiles
            .SelectMany(x => x.Commands)
            .Select(x => (fileId: x.FileId.ToLower(), localFile: x.LocalFilePath.ToLower()))
            .GroupBy(x => x.fileId)
            .Where(x => x.Count() > 1)
            .ToArray();

        if (multipleReferences.Length != 0)
        {
            string msg = multipleReferences.Aggregate("Multiple references to the same file" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.Location().LogError(msg);
            return StatusCode.Conflict;
        }

        return StatusCode.OK;
    }
}
