//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Azure;
//using Azure.Storage.Files.DataLake;
//using Microsoft.Extensions.Logging;
//using Toolbox.Data;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Azure;

//public static class DatalakeDeleteTool
//{
//    public static async Task<Option> Delete(this DataLakeFileClient _fileClient, ILogger logger, TrxSourceRecorder? trxRecorder = null, CancellationToken token = default)
//    {
//        _fileClient.NotNull();
//        logger.NotNull();

//        using var metric = logger.LogDuration("dataLakeStore-delete", "path={path}", _fileClient.Path);

//        logger.LogDebug("Deleting to {path}", _fileClient.Path);

//        try
//        {
//            Option<DataETag> readOption = StatusCode.NotFound;

//            if (trxRecorder != null)
//            {
//                readOption = await _fileClient.Get();
//                if (readOption.IsError()) return readOption.ToOptionStatus();
//            }

//            Response<bool> response = await _fileClient.DeleteIfExistsAsync(cancellationToken: token);

//            if (!response.Value)
//            {
//                logger.LogDebug("File path={path} does not exist", _fileClient.Path);
//                return StatusCode.NotFound;
//            }

//            //_datalakeStore.DataChangeLog.GetRecorder()?.Add(Path, readOption.Return());
//            return StatusCode.OK;
//        }
//        catch (RequestFailedException ex) when (ex.ErrorCode == "LeaseIdMissing")
//        {
//            logger.LogError(ex, "Failed to delete file {path}, LeaseIdMissing", _fileClient.Path);
//            return StatusCode.Locked;
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Failed to delete file {path}", _fileClient.Path);
//            return StatusCode.BadRequest;
//        }
//    }
//}
