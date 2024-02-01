using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Toolbox.Data;
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
        var context = new ScopeContext(_logger);

        BuildContext buildContext = Create(basePath, packageFile, context);

        buildContext = GetAllFiles(buildContext, context);
        buildContext = await VerifyConfigurationFiles(buildContext, context);
        buildContext = await ProcessManifestFiles(buildContext, context);
        buildContext = BuildDirectoryGraph(buildContext, context);
        buildContext = await SearchIndexTool.BuildSearchIndex(buildContext, context);
        buildContext = VerifyArticleIds(buildContext, context);
        buildContext = VerifyFileReferences(buildContext, context);

        if (buildContext.Errors.Count > 0)
        {
            context.LogError("Build failed, errors={errors}", buildContext.Errors.Join(';'));
            return StatusCode.Conflict;
        }

        CreatePackage(buildContext, context);
        return StatusCode.OK;
    }

    private BuildContext Create(string basePath, string packageFile, ScopeContext context)
    {
        string folderName = Path.GetFileNameWithoutExtension(packageFile) + "-obj";
        string workingFolder = Path.Combine(Path.GetDirectoryName(packageFile).NotNull(), folderName);

        if (Directory.Exists(workingFolder)) Directory.Delete(workingFolder, true);
        Directory.CreateDirectory(workingFolder);

        BuildContext buildContext = new BuildContext
        {
            BasePath = basePath,
            PackageFile = packageFile,
            WorkingFolder = workingFolder,
        };

        context.LogInformation(
            "Building package, basePath={basePath}, packageFile={packageFile}, buildFolder={buildFolder}",
            buildContext.BasePath, buildContext.PackageFile, buildContext.WorkingFolder
            );

        return buildContext;
    }

    private BuildContext GetAllFiles(BuildContext buildContext, ScopeContext context)
    {
        context.LogInformation("Scanning all files basePath={basePath}", buildContext.BasePath);

        string[] files = Directory.EnumerateFiles(buildContext.BasePath, "*.*", SearchOption.AllDirectories)
            .Where(x => x.IndexOf(".vscode") < 0)
            .Where(x => x.EndsWith(".json") || x.EndsWith(".md"))
            .ToArray();

        IReadOnlyList<(FileType fileType, string path)> fileList = files
            .Select(x => x switch
            {
                string v when v.EndsWith(NBlogConstants.ManifestExtension) => (FileType.Manifest, v),
                string v when v.EndsWith(NBlogConstants.ConfigurationExtension) => (FileType.Configuration, v),
                string v when v.EndsWith(NBlogConstants.ContactMeExtension) => (FileType.ContactMe, v),

                _ => (FileType.Unknown, null!),
            })
            .Where(x => x.Item1 != FileType.Unknown)
            .ToArray();

        buildContext = buildContext with
        {
            Files = fileList.ToSequence(),
            CopyCommands = fileList
                .Where(x => x.fileType == FileType.Configuration || x.fileType == FileType.ContactMe)
                .Select(x => x.fileType switch
                {
                    FileType.Configuration => (x.path, Path.GetFileName(x.path)),
                    FileType.ContactMe => (x.path, Path.GetFileName(x.path)),

                    _ => throw new ArgumentException($"Unknown file type={x.fileType}")
                }).ToSequence(),
        };

        context.LogInformation(
            "Found files in basePath={basePath}, file list count={fileListCount}, copyCommands.Count={copyCommandCount}",
            buildContext.BasePath, fileList.Count, buildContext.CopyCommands.Count
            );

        return buildContext;
    }

    private async Task<BuildContext> VerifyConfigurationFiles(BuildContext buildContext, ScopeContext context)
    {
        var configurationFiles = buildContext.Files
            .Where(x => x.FileType == FileType.Configuration)
            .Select(x => x.FilePath)
            .ToArray();

        context.LogInformation("Processing manifest files count={manifestFileCount}", configurationFiles.Length);

        IReadOnlyList<Option<NBlogConfiguration>> results = await ActionParallel.RunAsync(configurationFiles, readConfiguration);
        var e1 = results.Where(x => x.IsError()).Select(x => x.ToString()).ToArray();
        var configurations = results.Where(x => x.IsOk()).Select(x => x.Return()).ToArray();

        var result = buildContext with
        {
            Errors = buildContext.Errors + e1,
        };

        context.LogInformation("Processed manifest files count={configurationFiles}, errorCount={errorCount}", configurationFiles.Length, e1.Length);
        return result;


        async Task<Option<NBlogConfiguration>> readConfiguration(string file)
        {
            context.LogInformation("Reading and verifying configuration file={file}", file);
            string data = await File.ReadAllTextAsync(file);

            var o = data.ToObject<NBlogConfiguration>();
            if (o == null) return (StatusCode.BadRequest, $"Cannot deserialize json={file}");

            var v = o.Validate().Action(x => x.LogStatus(context, $"File={file}"));
            if (v.IsError()) return v.ToOptionStatus<NBlogConfiguration>();

            string fileId = Path.GetFileName(file);
            var verify = o.VerifyFileId(fileId, context);
            if (verify.IsError()) return (StatusCode.Conflict, $"DbName={o.DbName} does not match fileId={fileId}");

            string contactMeFile = file[0..(file.Length - NBlogConstants.ConfigurationExtension.Length)] + NBlogConstants.ContactMeExtension;
            if( !File.Exists(contactMeFile)) return (StatusCode.Conflict, $"ContactMe file={contactMeFile} does not exist");

            return o;
        }
    }

    private async Task<BuildContext> ProcessManifestFiles(BuildContext buildContext, ScopeContext context)
    {
        var manifests = buildContext.Files
            .Where(x => x.FileType == FileType.Manifest)
            .Select(x => x.FilePath)
            .ToArray();

        context.LogInformation("Processing manifest files count={manifestFileCount}", manifests.Length);

        IReadOnlyList<Option<ManifestFile>> results = await ActionParallel.RunAsync(manifests, async x => await ManifestFileTool.Read(x, buildContext.BasePath, context));
        var e1 = results.Where(x => x.IsError()).Select(x => x.ToString()).ToArray();
        var manifestFiles = results.Where(x => x.IsOk()).Select(x => x.Return()).ToArray();

        var c1 = manifestFiles
            .Select(x => (file: x.File, manifest: x.Manifest))
            .ToArray();

        var addMainifestCopyOption = ActionParallel.Run(c1, writeFile);
        var e2 = addMainifestCopyOption.Where(x => x.IsError()).Select(x => x.ToString()).ToArray();
        var addMainifestCopy = addMainifestCopyOption.Where(x => x.IsOk()).Select(x => x.Return()).ToArray();

        var addFiles = manifestFiles
            .SelectMany(x => x.Commands.Select(y => (y.LocalFilePath, y.FileId)))
            .ToArray();

        var result = buildContext with
        {
            ManifestFiles = manifestFiles.ToArray(),
            CopyCommands = buildContext.CopyCommands + addMainifestCopy + addFiles,
            Errors = buildContext.Errors + e1 + e2,
        };

        context.LogInformation("Processed manifest files count={manifestFileCount}", manifestFiles.Length);
        return result;


        Option<(string sourceFile, string zipFile)> writeFile((string file, ArticleManifest manifest) copy)
        {
            try
            {
                string workingFile = Path.Combine(buildContext.WorkingFolder, buildContext.RemoveBasePath(copy.file));
                context.LogInformation("Writing modified manifest sourceFile={sourceFile} -> workingFile={workingFile}", copy.file, workingFile);

                Path.GetDirectoryName(workingFile)?.Action(x => Directory.CreateDirectory(x));
                string json = copy.manifest.ToJson();
                File.WriteAllText(workingFile, json);

                return (workingFile, copy.manifest.ArticleId);
            }
            catch (Exception ex)
            {
                return (StatusCode.Conflict, $"Failed to copy, ex={ex.Message}");
            }
        }
    }

    private BuildContext BuildDirectoryGraph(BuildContext buildContext, ScopeContext context)
    {
        context.LogInformation("Building direcvtory graph");

        Option<GraphMap> indexOption = new ArticleDirectoryBuilder()
            .Add(buildContext.ManifestFiles.Select(x => x.Manifest))
            .Build();

        if (indexOption.IsError())
        {
            context.Location().LogError("Failed to build article directory graph, error={error}", indexOption);
            return buildContext with { Errors = buildContext.Errors + $"Failed to build article directory graph, error={indexOption}" };
        }

        var index = indexOption.Return();

        string sourceFile = buildContext.CreateWorkFile(NBlogConstants.DirectoryActorKey);
        string json = index.ToJson();
        File.WriteAllText(sourceFile, json);
        context.LogInformation("Writing index to file={file}", sourceFile);

        buildContext = buildContext with
        {
            CopyCommands = buildContext.CopyCommands + (sourceFile, NBlogConstants.DirectoryActorKey),
        };

        context.LogInformation("Completed building, nodes.count={nodesCount}, edges.count={edgesCount}", index.Nodes.Count, index.Edges.Count);
        return buildContext;
    }

    private BuildContext VerifyArticleIds(BuildContext buildContext, ScopeContext context)
    {
        context.LogInformation("Verifying article Ids...");

        var fileIds = buildContext.ManifestFiles.Select(x => x.Manifest.ArticleId).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (fileIds.Length != buildContext.ManifestFiles.Count)
        {
            string msg = fileIds.Aggregate("Duplicate 'Article Ids'" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.Location().LogError(msg);
            return buildContext with { Errors = buildContext.Errors + msg };
        }

        return buildContext;
    }

    private BuildContext VerifyFileReferences(BuildContext buildContext, ScopeContext context)
    {
        context.LogInformation("Verifying File references ...");

        var multipleReferences = buildContext.ManifestFiles
            .SelectMany(x => x.Commands)
            .Select(x => (fileId: x.FileId.ToLower(), localFile: x.LocalFilePath.ToLower()))
            .GroupBy(x => x.fileId)
            .Where(x => x.Count() > 1)
            .ToArray();

        if (multipleReferences.Length != 0)
        {
            string msg = multipleReferences.Aggregate("Multiple references to the same file" + Environment.NewLine, (a, x) => a += x + Environment.NewLine);
            context.Location().LogError(msg);
            return buildContext with { Errors = buildContext.Errors + msg };
        }

        return buildContext;
    }

    private void CreatePackage(BuildContext buildContext, ScopeContext context)
    {
        context.LogInformation("Creating package={packageFile}", buildContext.PackageFile);

        using var zipFile = File.Open(buildContext.PackageFile, FileMode.Create);
        using var zip = new ZipArchive(zipFile, ZipArchiveMode.Create);

        foreach (var item in buildContext.CopyCommands)
        {
            zip.CreateEntryFromFile(item.sourceFile, item.zipFile.ToLower());
            context.LogInformation("Writing sourceFile={sourceFile} to zipEntry={zipEntry}", item.sourceFile, item.zipFile);
        }
    }
}
