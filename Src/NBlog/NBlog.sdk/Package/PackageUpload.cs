using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Azure.DataLake;
using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public class PackageUpload
{
    private readonly ILogger<PackageUpload> _logger;
    private readonly IServiceProvider _service;

    public PackageUpload(IServiceProvider service, ILogger<PackageUpload> logger)
    {
        _service = service.NotNull();
        _logger = logger.NotNull();
    }

    public async Task<Option> Upload(string packageFile, DatalakeOption datalakeOption)
    {
        packageFile.NotEmpty();
        if (!datalakeOption.Validate(out var v)) return v;

        packageFile = PathTools.SetExtension(packageFile, NBlogConstants.PackageExtension);
        var context = new ScopeContext(_logger);

        context.Location().LogInformation("Uploading package={packageFile} to datalake, account={account}, container={container}, basePath={basePath}",
            packageFile, datalakeOption.Account, datalakeOption.Container, datalakeOption.BasePath);

        if (!File.Exists(packageFile)) return (StatusCode.BadRequest, $"Package file={packageFile} does not exist");
        IDatalakeStore datalakeStore = ActivatorUtilities.CreateInstance<DatalakeStore>(_service, datalakeOption);

        Option clearOption = await ClearDatalake(datalakeStore, context);

        using var zipFile = File.Open(packageFile, FileMode.Open, FileAccess.Read);
        using var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Read);

        //Option manifestOption = await ProcessManifests(zipArchive, datalakeStore, context);
        //if (manifestOption.IsError()) return manifestOption;

        Option datafileOption = await ProcessFiles(zipArchive, datalakeStore, context);
        if (datafileOption.IsError()) return datafileOption;

        return StatusCode.OK;
    }

    private async Task<Option> ClearDatalake(IDatalakeStore datalakeStore, ScopeContext context)
    {
        var query = new QueryParameter();
        int maxCount = 100;

        context.Location().LogInformation("Clearing all files in datalake storage");

        while (maxCount-- > 0)
        {
            var queryResponseOption = await datalakeStore.Search(query, context);
            if (queryResponseOption.IsError())
            {
                context.Location().LogError("Failed to enumerate files on datalake");
                return queryResponseOption.ToOptionStatus();
            }

            QueryResponse<DatalakePathItem> queryResponse = queryResponseOption.Return();
            if (queryResponse.Items.Count == 0 || queryResponse.EndOfSearch) return StatusCode.OK;

            var directories = queryResponse.Items.Where(x => x.IsDirectory == true).ToArray();
            if (directories.Length > 0)
            {
                foreach (DatalakePathItem file in directories)
                {
                    context.Location().LogInformation("Deleting directory={directoryName}", file.Name);
                    await datalakeStore.DeleteDirectory(file.Name, context);
                }

                continue;
            }

            foreach (DatalakePathItem file in queryResponse.Items)
            {
                context.Location().LogInformation("Deleting file={file}", file.Name);
                var status = await datalakeStore.Delete(file.Name, context);
            }
        }

        return (StatusCode.Conflict, "maxCount exceeded");
    }

    private async Task<Option> ProcessFiles(ZipArchive zipArchive, IDatalakeStore datalakeStore, ScopeContext context)
    {
        context.Location().LogInformation("Uploading files to datalake storage");

        foreach (var zipFile in zipArchive.Entries)
        {
            byte[] data;
            using (var zipStream = zipFile.Open())
            using (var memory = new MemoryStream())
            {
                zipStream.CopyTo(memory);
                data = memory.ToArray();
            }

            string dataLakePath = calcDatalakePath( zipFile.FullName);
            var dataEtag = new DataETag(data);
            var writeOption = await datalakeStore.Write(dataLakePath, dataEtag, true, context);
            if (writeOption.IsError())
            {
                context.Location().LogError("Cannot write to datalake, path={path}, error={error}", dataLakePath, writeOption.Error);
                return writeOption.ToOptionStatus();
            }

            context.Location().LogInformation("Write fileId={fileId} to datalakePath={datalakePath}", zipFile.FullName, dataLakePath);
        }

        return StatusCode.OK;

        string calcDatalakePath(string path) => path switch
        {
            string v when v.StartsWith(PackageBuild.ManifestFilesFolder) => v[PackageBuild.ManifestFilesFolder.Length..],
            string v when v.StartsWith(PackageBuild.DataFilesFolder) => v[PackageBuild.DataFilesFolder.Length..],
            string v when v == (PackageBuild.ArticleIndexZipFile) => v,

            _ => throw new ArgumentException($"Unknown path or path prefix: {path}")
        };
    }
}
