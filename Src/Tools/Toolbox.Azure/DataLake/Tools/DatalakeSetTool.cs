using Azure;
using Azure.Storage.Files.DataLake;
using Azure.Storage.Files.DataLake.Models;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public static class DatalakeSetTool
{
    public static Task<Option<string>> Add(this DataLakeFileClient fileClient, DataETag dataETag, ScopeContext context)
    {
        return Upload(fileClient, false, dataETag, null, context);
    }

    public static Task<Option<string>> Set(this DataLakeFileClient fileClient, DataETag dataETag, ScopeContext context)
    {
        return Upload(fileClient, true, dataETag, null, context);
    }

    public static Task<Option<string>> Set(this DataLakeFileClient fileClient, DatalakeLeasedAccess datalakeLease, DataETag dataETag, ScopeContext context)
    {
        return Upload(fileClient, true, dataETag, datalakeLease.LeaseId, context);
    }

    private static async Task<Option<string>> Upload(DataLakeFileClient fileClient, bool overwrite, DataETag dataETag, string? leaseId, ScopeContext context)
    {
        Response<PathInfo> result;
        context.Location().LogTrace($"Writing (Upload) to {fileClient.Path}, data.Length={dataETag.Data.Length}, eTag={dataETag.ETag?.ToString() ?? "<null>"}");
        dataETag.NotNull().Assert(x => x.Data.Length > 0, $"length must be greater then 0, path={fileClient.Path}");

        using var metric = context.LogDuration("dataLakeStore-upload", "path={path}", fileClient.Path);
        using var fromStream = new MemoryStream(dataETag.Data.ToArray());

        try
        {
            if (dataETag.ETag != default || leaseId.IsNotEmpty())
            {
                var option = new DataLakeFileUploadOptions
                {
                    Conditions = new DataLakeRequestConditions
                    {
                        IfMatch = leaseId.IsEmpty() && dataETag.ETag.IsNotEmpty() ? new ETag(dataETag.ETag) : null,
                        LeaseId = leaseId
                    }
                };

                result = await fileClient.UploadAsync(fromStream, option, context).ConfigureAwait(false);
                return result.Value.ETag.ToString();
            }

            result = await fileClient.UploadAsync(fromStream, overwrite, context).ConfigureAwait(false);
            return result.Value.ETag.ToString();
        }
        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing" || ex.ErrorCode == "LeaseIdMismatch" || ex.ErrorCode == "LeaseNotPresent")
        {
            context.Location().LogError(ex, "Failed to upload, 'RequestFailedException.ErrorCode == {errorCode}', path={path}, message={message}", ex.ErrorCode, fileClient.Path, ex.Message);
            return (StatusCode.Locked, "LeaseIdMissing");
        }
        catch (Exception ex)
        {
            context.Location().LogError(ex, "Failed to upload {path}, exType={exType}, message={message}", fileClient.Path, ex.GetType(), ex.Message);
            return (StatusCode.InternalServerError, ex.Message.ToSafeLoggingFormat());
        }
    }
}
