using System.Collections.Concurrent;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class PackageBuild
{
    public const string ManifestFilesFolder = "manifestfiles/";
    public const string DataFilesFolder = "datafiles/";
    public const string ArticleIndexZipFile = NBlogConstants.DirectoryActorKey;

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

        WriteManifestFilesToZip(zip, manifestFileList.Files, context);
        WriteFilesToZip(zip, manifestFileList.Files, context);
        BuildAndWriteIndex(zip, manifestFileList.Files, context);

        context.Location().LogInformation("Completed: Package has been created, files added={count}", manifestFileList.Files.Count);
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

                string manifestFileEntry = ManifestFilesFolder + queueManifest.Manifest.ArticleId;
                zip.CreateEntryFromFile(tempFile, manifestFileEntry.ToLower());
                context.Location().LogInformation("Writing manifest file={file} to zipEntry={zipEntry}", queueManifest.File, manifestFileEntry);
            }

            context.Location().LogInformation("Write manifest count={count} files", manifestFiles.Count);
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
            string fileEntry = DataFilesFolder + dataFile.fileId;
            zip.CreateEntryFromFile(dataFile.localFile, fileEntry.ToLower());
            context.Location().LogInformation("Writing data command file={file} to zipEntry={zipEntry}", dataFile.localFile, fileEntry);
        }

        context.Location().LogInformation("Write data count={count} files", dataFiles.Length);
    }

    private static void BuildAndWriteIndex(ZipArchive zip, IReadOnlyList<ManifestFile> manifestFiles, ScopeContext context)
    {
        var map = new GraphMap();

        manifestFiles.SelectMany(x => x.Manifest.GetNodes()).ForEach(x => map.Nodes.Add(x, true).ThrowOnError());
        manifestFiles.SelectMany(x => x.Manifest.GetEdges()).ForEach(x => map.Edges.Add(x, true).ThrowOnError());

        string tempFile = Path.GetTempFileName();

        try
        {
            string json = map.ToJson();
            File.WriteAllText(tempFile, json);
            zip.CreateEntryFromFile(tempFile, ArticleIndexZipFile);

            context.Location().LogInformation("Writing directory to zipEntry={zipEntry}", ArticleIndexZipFile);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
