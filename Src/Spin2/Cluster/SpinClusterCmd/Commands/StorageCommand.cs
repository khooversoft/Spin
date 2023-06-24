using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Actors.Storage;
using SpinCluster.sdk.Client;
using Toolbox.Data;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal class StorageCommand : CommandAbstract<StorageBlob>
{
    private readonly static DataType<StorageBlob> _dataType = new DataType<StorageBlob>
    {
        Name = "storage",
        Validator = StorageBlobValidator.Validator,
        GetKey = x => x.ObjectId,
    };

    public StorageCommand(SpinClusterClient client, ILogger<StorageCommand> logger)
        : base("storage", "Manage data in storage", _dataType, client, logger)
    {
        AddCommand(UploadCommand());
        AddCommand(DownloadCommand());
    }

    private Command UploadCommand()
    {
        var cmd = new Command("upload", "Upload a file to storage");

        Argument<string> idArgument = new Argument<string>("objectId", $"ObjectId of the {_dataType.Name}, syntax={ObjectId.Syntax}");
        Argument<string> fileArgument = new Argument<string>("file", "File to upload");

        cmd.AddArgument(idArgument);
        cmd.AddArgument(fileArgument);

        cmd.SetHandler(async (objectId, file) =>
        {
            var context = new ScopeContext(_logger);

            if (!ObjectId.IsValid(objectId))
            {
                _logger.LogError("Invalid objectId={objectId}, syntax={syntax}", objectId, ObjectId.Syntax);
                return;
            }

            if (!File.Exists(file))
            {
                context.Location().LogError("File {file} does not exist", file);
                return;
            }

            byte[] content = File.ReadAllBytes(file);

            StorageBlob blob = new StorageBlobBuilder()
                .SetObjectId(objectId)
                .SetContent(content)
                .Build();

            context.Location().LogInformation("Uploading blob={blob}", blob.ToString());
            StatusCode statusCode = await _client.Set<StorageBlob>(objectId, blob, context);

            context.Location().Log(statusCode.IsOk() ? LogLevel.Information : LogLevel.Error, "Set file={file}, statusCode={statusCode}", file, statusCode);

        }, idArgument, fileArgument);
        return cmd;
    }

    private Command DownloadCommand()
    {
        var cmd = new Command("download", "Download a file from storage");

        Argument<string> idArgument = new Argument<string>("objectId", $"ObjectId of the {_dataType.Name}, syntax={ObjectId.Syntax}");
        Argument<string> fileArgument = new Argument<string>("file", "File to write download data to");

        cmd.AddArgument(idArgument);
        cmd.AddArgument(fileArgument);

        cmd.SetHandler(async (objectId, file) =>
        {
            var context = new ScopeContext(_logger);

            if (!ObjectId.IsValid(objectId))
            {
                _logger.LogError("Invalid objectId={objectId}, syntax={syntax}", objectId, ObjectId.Syntax);
                return;
            }

            Toolbox.Types.Option<StorageBlob> blob = await _client.Get<StorageBlob>(objectId, context);
            if (blob.IsError())
            {
                context.Location().LogError("Could not download objectId={objectId}", objectId);
                return;
            }

            if (!blob.Return().IsHashVerify())
            {
                context.Location().LogError("Storage blob hash does not verify, objectId={objectId}", objectId);
                return;
            }

            File.WriteAllBytes(file, blob.Return().Content);

            context.Location().LogInformation("Downloaded file={file} from storage", file);

        }, idArgument, fileArgument);

        return cmd;
    }
}
