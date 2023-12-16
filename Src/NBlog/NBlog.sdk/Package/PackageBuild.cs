using System.Collections.Concurrent;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class PackageBuild
{
    public const string ManifestFilesFolder = "manifestfiles/";
    public const string DataFilesFolder = "datafiles/";

    private readonly ILogger<PackageBuild> _logger;
    public PackageBuild(ILogger<PackageBuild> logger) => _logger = logger.NotNull();

    public async Task<Option> Build(string basePath, string packageFile)
    {
        packageFile = PathTools.SetExtension(packageFile, NBlogConstants.PackageExtension);
        var context = new ScopeContext(_logger);
        context.Location().LogInformation("Building package, basePath={basePath}, packageFile={packageFile}", basePath, packageFile);

        IReadOnlyList<QueuedManifest> manifestFiles = await ReadManifestFiles(basePath, context);
        if (manifestFiles.Count == 0) return (StatusCode.NoContent);
        context.Location().LogInformation("Manifest detected, files added={count}", manifestFiles.Count);

        var verifyOption = VerifyManifest(manifestFiles, context);
        if (verifyOption.IsError()) return verifyOption;

        using var zipFile = File.Open(packageFile, FileMode.Create);
        using var zip = new ZipArchive(zipFile, ZipArchiveMode.Create);

        WriteManifestFilesToZip(zip, manifestFiles, context);
        WriteFilesToZip(zip, manifestFiles, context);

        context.Location().LogInformation("Completed: Package has been created, files added={count}", manifestFiles.Count);
        return StatusCode.OK;
    }

    private static async Task ReadManifest(string file, ConcurrentQueue<QueuedManifest> queue, ScopeContext context)
    {
        string data = await File.ReadAllTextAsync(file);
        var model = data.ToObject<ArticleManifest>();
        if (model == null || model.Validate().IsError())
        {
            context.Location().LogError("File={file} is not a valid manifest file", file);
            return;
        }

        string folder = Path.GetFullPath(file)
            .Func(x => Path.GetDirectoryName(x))
            .NotNull($"File={file} does not have directory name");

        var commands = model.GetCommands()
            .Select(x => x with { LocalFilePath = Path.Combine(folder, x.LocalFilePath) })
            .ToArray();

        if (commands.Length == 0)
        {
            context.Location().LogError("Manifest={file} does not have any commands specified");
            return;
        }

        var findResult = commands
            .Select(x => File.Exists(x.LocalFilePath) ? null : $"File={x.LocalFilePath} does not exist, local file for manifest={file}")
            .OfType<string>()
            .ToArray();

        if (findResult.Length != 0)
        {
            string msg = findResult.Aggregate("Cannot find local files" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.Location().LogError(msg);
            return;
        }

        var queuedManifest = new QueuedManifest
        {
            File = file,
            Commands = commands,
            Manifest = model,
        };

        queue.Enqueue(queuedManifest);
    }

    private async Task<IReadOnlyList<QueuedManifest>> ReadManifestFiles(string basePath, ScopeContext context)
    {
        var queue = new ConcurrentQueue<QueuedManifest>();

        string[] files = Directory.GetFiles(basePath, "*.json", SearchOption.AllDirectories);
        context.Location().LogInformation("Reading manifest files, count={count}", files.Length);
        if (files.Length == 0)
        {
            context.Location().LogWarning("No files to read");
            return Array.Empty<QueuedManifest>();
        }

        await ActionBlockParallel.Run<string>(async x => await ReadManifest(x, queue, context), files);

        if (files.Length != queue.Count)
        {
            context.Location().LogError("Failed to read {count} files.", files.Length - queue.Count);
            return Array.Empty<QueuedManifest>();
        }

        context.Location().LogInformation("Read {count} files", queue.Count);
        return queue.ToArray();
    }

    private Option VerifyManifest(IReadOnlyList<QueuedManifest> manifestFiles, ScopeContext context)
    {
        var fileIds = manifestFiles.Select(x => x.Manifest.ArticleId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (fileIds.Length != manifestFiles.Count)
        {
            string msg = fileIds.Aggregate("Duplicate 'Article Ids'" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.Location().LogError(msg);
            return StatusCode.Conflict;
        }

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

    private void WriteManifestFilesToZip(ZipArchive zip, IReadOnlyList<QueuedManifest> manifestFiles, ScopeContext context)
    {
        foreach (var queueManifest in manifestFiles)
        {
            string zipFolder = ManifestFilesFolder + queueManifest.Manifest.ArticleId;

            string manifestFileEntry = zipFolder + "/" + Path.GetFileName(queueManifest.File);
            zip.CreateEntryFromFile(queueManifest.File, manifestFileEntry.ToLower());
            context.Location().LogInformation("Writing manifest file={file} to zipEntry={zipEntry}", queueManifest.File, manifestFileEntry);
        }

        context.Location().LogInformation("Write manifest count={count} files", manifestFiles.Count);
    }

    private void WriteFilesToZip(ZipArchive zip, IReadOnlyList<QueuedManifest> manifestFiles, ScopeContext context)
    {
        var dataFiles = manifestFiles
            .SelectMany(x => x.Commands)
            .Select(x => (fileId: x.FileId, localFile: x.LocalFilePath))
            .ToArray();

        foreach (var dataFile in dataFiles)
        {
            string fileEntry = DataFilesFolder + dataFile.fileId;
            zip.CreateEntryFromFile(dataFile.localFile, fileEntry.ToLower());
            context.Location().LogInformation("Writing data command file={file} to zipEntry={zipEntry}", dataFile.localFile, fileEntry);
        }

        context.Location().LogInformation("Write data count={count} files", manifestFiles.Count);
    }

    private record QueuedManifest
    {
        public string File { get; init; } = null!;
        public IReadOnlyList<CommandNode> Commands { get; init; } = Array.Empty<CommandNode>();
        public ArticleManifest Manifest { get; init; } = null!;
    }
}
