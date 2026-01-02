//using Azure;
//using Azure.Storage.Files.DataLake;
//using Azure.Storage.Files.DataLake.Models;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Azure;

//public static class DatalakeSetTool
//{
//    public static Task<Option<string>> Add(this DataLakeFileClient fileClient, DataETag dataETag, ILogger logger, CancellationToken token = default)
//    {
//        return Upload(fileClient, false, dataETag, null, logger, token);
//    }

//    public static Task<Option<string>> Set(this DataLakeFileClient fileClient, DataETag dataETag, ILogger logger, CancellationToken token = default)
//    {
//        return Upload(fileClient, true, dataETag, null, logger, token);
//    }

//    public static Task<Option<string>> Set(this DataLakeFileClient fileClient, DatalakeLeasedAccess datalakeLease, DataETag dataETag, ILogger logger, CancellationToken token = default)
//    {
//        return Upload(fileClient, true, dataETag, datalakeLease.LeaseId, logger, token);
//    }

//    private static async Task<Option<string>> Upload(DataLakeFileClient fileClient, bool overwrite, DataETag dataETag, string? leaseId, ILogger logger, CancellationToken token)
//    {
//        Response<PathInfo> result;
//        logger.LogDebug("Writing (Upload) to path={path}, length={length}, eTag={etag}", fileClient.Path, dataETag.Data.Length, dataETag.ETag?.ToString() ?? "<null>");
//        dataETag.NotNull().Assert(x => x.Data.Length > 0, $"length must be greater then 0, path={fileClient.Path}");

//        using var metric = logger.LogDuration("dataLakeStore-upload", "path={path}", fileClient.Path);
//        using var fromStream = new MemoryStream(dataETag.Data.ToArray());

//        try
//        {
//            if (dataETag.ETag != default || leaseId.IsNotEmpty())
//            {
//                var option = new DataLakeFileUploadOptions
//                {
//                    Conditions = new DataLakeRequestConditions
//                    {
//                        IfMatch = leaseId.IsEmpty() && dataETag.ETag.IsNotEmpty() ? new ETag(dataETag.ETag) : null,
//                        LeaseId = leaseId
//                    }
//                };

//                result = await fileClient.UploadAsync(fromStream, option, token);
//                return result.Value.ETag.ToString();
//            }

//            result = await fileClient.UploadAsync(fromStream, overwrite, token);
//            return result.Value.ETag.ToString();
//        }
//        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing" || ex.ErrorCode == "LeaseIdMismatch" || ex.ErrorCode == "LeaseNotPresent")
//        {
//            logger.LogError(ex, "Failed to upload, 'RequestFailedException.ErrorCode == {errorCode}', path={path}, message={message}", ex.ErrorCode, fileClient.Path, ex.Message);
//            return (StatusCode.Locked, "LeaseIdMissing");
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Failed to upload {path}, exType={exType}, message={message}", fileClient.Path, ex.GetType(), ex.Message);
//            return (StatusCode.InternalServerError, ex.Message.ToSafeLoggingFormat());
//        }
//    }
//}
