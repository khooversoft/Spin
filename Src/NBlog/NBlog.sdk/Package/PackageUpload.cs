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

        Option manifestOption = await ProcessManifests(zipArchive, datalakeStore, context);
        if (manifestOption.IsError()) return manifestOption;

        Option datafileOption = await ProcessDataFiles(zipArchive, datalakeStore, context);
        if (datafileOption.IsError()) return datafileOption;

        return StatusCode.OK;
    }

    private async Task<Option> ClearDatalake(IDatalakeStore datalakeStore, ScopeContext context)
    {
        var query = new QueryParameter();
        int maxCount = 100;

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
                    await datalakeStore.DeleteDirectory(file.Name, context);
                }

                continue;
            }

            foreach (DatalakePathItem file in queryResponse.Items)
            {
                StatusCode status = file.IsDirectory switch
                {
                    true => await datalakeStore.DeleteDirectory(file.Name, context),
                    _ => await datalakeStore.Delete(file.Name, context)
                };

                if (status.IsError()) return status;
            }
        }

        return (StatusCode.Conflict, "maxCount exceeded");
    }

    private async Task<Option> ProcessManifests(ZipArchive zipArchive, IDatalakeStore datalakeStore, ScopeContext context)
    {
        ZipArchiveEntry[] zipFiles = zipArchive.Entries
            .Where(x => x.FullName.StartsWith(PackageBuild.ManifestFilesFolder) && x.FullName.EndsWith(".json"))
            .ToArray();

        foreach (var zipFile in zipFiles)
        {
            byte[] data;
            using (var zipStream = zipFile.Open())
            using (var memory = new MemoryStream())
            {
                zipStream.CopyTo(memory);
                data = memory.ToArray();
            }

            var model = data.BytesToString().ToObject<ArticleManifest>();
            if (model == null) return (StatusCode.NotFound, $"zip entry={zipFile.FullName} failed to deserialize to 'ArticleManifest'");
            if (!model.Validate(out var r)) return r;

            string dataLakePath = zipFile.FullName[PackageBuild.ManifestFilesFolder.Length..];
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
    }

    private async Task<Option> ProcessDataFiles(ZipArchive zipArchive, IDatalakeStore datalakeStore, ScopeContext context)
    {
        ZipArchiveEntry[] zipFiles = zipArchive.Entries
            .Where(x => x.FullName.StartsWith(PackageBuild.DataFilesFolder))
            .ToArray();

        foreach (var zipFile in zipFiles)
        {
            byte[] data;
            using (var zipStream = zipFile.Open())
            using (var memory = new MemoryStream())
            {
                zipStream.CopyTo(memory);
                data = memory.ToArray();
            }

            string dataLakePath = zipFile.FullName[PackageBuild.DataFilesFolder.Length..];
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
    }
}
