using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
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

        IReadOnlyList<QueuedManifest> manifestFiles = await ReadManifestFiles(basePath, context);
        if (manifestFiles.Count == 0) return (StatusCode.NoContent);
        context.Location().LogInformation("Completed: Package has been created, files added={count}", manifestFiles.Count);

        var verifyOption = VerifyManifest(manifestFiles, context);
        if (verifyOption.IsError()) return verifyOption;

        context.Location().LogInformation("Creating NBlog package file={file}", packageFile);
        using var zipFile = File.Open(packageFile, FileMode.Create);
        using var zip = new ZipArchive(zipFile, ZipArchiveMode.Create);

        foreach (var manifest in manifestFiles)
        {
            WriteManifestAndFilesToZip(zip, manifest, context);
        }

        context.Location().LogInformation("Completed: Package has been created, files added={count}", manifestFiles.Count);
        return StatusCode.OK;
    }

    private static async Task ReadManifest(string file, ConcurrentQueue<QueuedManifest> queue, ScopeContext context)
    {
        string data = await File.ReadAllTextAsync(file);
        var model = data.ToObject<ArticleManifest>();
        if (model == null)
        {
            context.Location().LogInformation("File={file} is not a valid manifest file", file);
            return;
        }

        if (!model.Validate(out Option v))
        {
            context.Location().LogError("Manifest file={file} does not validate, error={error}", file, v.Error);
            return;
        }

        var commands = model.GetCommands();
        if (commands.Count == 0)
        {
            context.Location().LogError("Manifest={file} does not have any commands specified");
            return;
        }

        string folder = Path.GetDirectoryName(file).NotNull($"File={file} does not have directory name");
        var commandFiles = commands.Select(x => Path.Combine(folder, x.LocalFilePath)).ToArray();

        var findResult = commandFiles
            .Select(x => File.Exists(x) ? null : $"File={x} does not exist, local file for manifest={file}")
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
            CommandLocalFiles = commandFiles,
            Manifest = model,
        };

        queue.Enqueue(queuedManifest);
    }

    private async Task<IReadOnlyList<QueuedManifest>> ReadManifestFiles(string basePath, ScopeContext context)
    {
        var options = new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 };
        var queue = new ConcurrentQueue<QueuedManifest>();
        var block = new ActionBlock<string>(async x => await ReadManifest(x, queue, context), options);

        string[] files = Directory.GetFiles(basePath, "*.json", SearchOption.AllDirectories);
        context.Location().LogInformation("Reading manifest files, count={count}", files.Length);
        if (files.Length == 0)
        {
            context.Location().LogWarning("No files to read");
            return Array.Empty<QueuedManifest>();
        }

        await files.ForEachAsync(async x => await block.SendAsync(x));
        block.Complete();
        await block.Completion;

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
        if (fileIds.Length == manifestFiles.Count) return StatusCode.OK;

        string msg = fileIds.Aggregate("Duplicate 'Article Ids" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
        context.Location().LogError(msg);
        return StatusCode.Conflict;
    }

    private void WriteManifestAndFilesToZip(ZipArchive zip, QueuedManifest queueManifest, ScopeContext context)
    {
        string zipFolder = queueManifest.Manifest.ArticleId;

        string manifestFileEntry = zipFolder + "/" + Path.GetFileName(queueManifest.File);
        zip.CreateEntryFromFile(queueManifest.File, manifestFileEntry);
        context.Location().LogInformation("Writing manifest file={file} to zipEntry={zipEntry}", queueManifest.File, manifestFileEntry);

        var commands = queueManifest.Manifest.GetCommands();
        foreach (var commandLocalFile in queueManifest.CommandLocalFiles)
        {
            string localFileEntry = zipFolder + "/" + Path.GetFileName(commandLocalFile);
            zip.CreateEntryFromFile(commandLocalFile, localFileEntry);
            context.Location().LogInformation("Writing manifest command file={file} to zipEntry={zipEntry}", commandLocalFile, localFileEntry);
        }
    }

    private record QueuedManifest
    {
        public string File { get; init; } = null!;
        public IReadOnlyList<string> CommandLocalFiles { get; init; } = Array.Empty<string>();
        public ArticleManifest Manifest { get; init; } = null!;
    }
}
