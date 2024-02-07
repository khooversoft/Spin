using System.Diagnostics;
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

        context.LogInformation("Uploading package={packageFile} to datalake, account={account}, container={container}, basePath={basePath}",
            packageFile, datalakeOption.Account, datalakeOption.Container, datalakeOption.BasePath);

        if (!File.Exists(packageFile))
        {
            context.LogError("Package file={packageFile} does not exist.", packageFile);
            return StatusCode.BadRequest;
        }

        IDatalakeStore datalakeStore = ActivatorUtilities.CreateInstance<DatalakeStore>(_service, datalakeOption);

        Option clearOption = await ClearDatalake(datalakeStore, context);

        using var zipFile = File.Open(packageFile, FileMode.Open, FileAccess.Read);
        using var zipArchive = new ZipArchive(zipFile, ZipArchiveMode.Read);

        Option datafileOption = await ProcessFiles(zipArchive, datalakeStore, context);
        if (datafileOption.IsError()) return datafileOption;

        return StatusCode.OK;
    }

    private async Task<Option> ClearDatalake(IDatalakeStore datalakeStore, ScopeContext context)
    {
        var query = new QueryParameter();

        context.Location().LogInformation("Clearing all files in datalake storage");

        var queryResponseOption = await datalakeStore.Search(query, context);
        if (queryResponseOption.IsError())
        {
            context.Location().LogError("Failed to enumerate files on datalake");
            return queryResponseOption.ToOptionStatus();
        }

        QueryResponse<DatalakePathItem> queryResponse = queryResponseOption.Return();
        if (queryResponse.Items.Count == 0 || queryResponse.EndOfSearch) return StatusCode.OK;

        var deleteResults = await ActionParallel.RunAsync(queryResponse.Items, deleteItem);
        if (deleteResults.Any(x => x.IsError())) return StatusCode.Conflict;

        return StatusCode.OK;

        async Task<Option> deleteItem(DatalakePathItem pathItem)
        {
            switch (pathItem)
            {
                case { IsDirectory: true } file when file.Name.StartsWith(NBlogConstants.ContactRequestFolder):
                    context.LogInformation("Skipping delete of directory={directoryName} in datalake", file.Name);
                    return StatusCode.OK;

                case { IsDirectory: true } file:
                    context.LogInformation("Deleting directory={directoryName} in datalake", file.Name);
                    return await datalakeStore.DeleteDirectory(file.Name, context);

                case { IsDirectory: false } file:
                    context.LogInformation("Deleting file={file} in datalake", file.Name);
                    return await datalakeStore.Delete(file.Name, context);

                default: throw new UnreachableException();
            }
        }
    }

    private async Task<Option> ProcessFiles(ZipArchive zipArchive, IDatalakeStore datalakeStore, ScopeContext context)
    {
        context.Location().LogInformation("Uploading files to datalake storage");

        await ActionParallel.Run<(string datalakePath, DataETag dataEtag)>(writeToDatalake, readEntries(), 5);
        return StatusCode.OK;

        IEnumerable<(string datalakePath, DataETag dataEtag)> readEntries()
        {
            foreach (var zipFile in zipArchive.Entries)
            {
                byte[] data;
                using (var zipStream = zipFile.Open())
                using (var memory = new MemoryStream())
                {
                    zipStream.CopyTo(memory);
                    data = memory.ToArray();
                }

                if (data.Length == 0)
                {
                    context.Location().LogError("Data length is 0 for zipFile={zipFile}", zipFile);
                    continue;
                }

                context.LogInformation("Writting fileId={fileId} to datalakePath", zipFile.FullName);
                var dataEtag = new DataETag(data);

                yield return (zipFile.FullName, dataEtag);
            }
        }

        async Task writeToDatalake((string datalakePath, DataETag dataEtag) payload)
        {
            var writeOption = await datalakeStore.Write(payload.datalakePath, payload.dataEtag, true, context);
            if (writeOption.IsError())
            {
                context.Location().LogError("Cannot write to datalake, path={path}, error={error}", payload.datalakePath, writeOption.Error);
                return;
            }
        }
    }
}
