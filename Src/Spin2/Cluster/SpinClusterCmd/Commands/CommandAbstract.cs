using System.CommandLine;
using Microsoft.Extensions.Logging;
using SpinCluster.sdk.Client;
using SpinClusterCmd.Application;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinClusterCmd.Commands;

internal interface ICommandAbstract
{
}

internal readonly record struct DataType<T>
{
    public string Name { get; init; }
    public Validator<T> Validator { get; init; }
    public Func<T, ObjectId> GetKey { get; init; }
}


internal class CommandAbstract<T> : Command, ICommandAbstract
{
    protected readonly SpinClusterClient _client;
    protected readonly ILogger _logger;
    private readonly DataType<T> _dataType;

    public CommandAbstract(string command, string description, DataType<T> dataType, SpinClusterClient client, ILogger logger)
        : base(command, description)
    {
        _dataType = dataType.NotNull();
        _client = client.NotNull();
        _logger = logger.NotNull();

        AddCommand(SetCommand());
        AddCommand(GetCommand());
        AddCommand(DeleteCommand());
    }

    private Command SetCommand()
    {
        var cmd = new Command("set", $"Create or update {_dataType.Name}");
        Argument<string> file = new Argument<string>("file", $"Json file for {_dataType.Name} data");

        cmd.AddArgument(file);

        cmd.SetHandler(async (file) =>
        {
            var context = new ScopeContext(_logger);
            if (!File.Exists(file))
            {
                context.Location().LogError("File {file} does not exist", file);
                return;
            }

            T model = JsonFileTools.Read<T>(file);
            if (!_dataType.Validator.Validate(model).IsValid)
            {
                string errors = _dataType.Validator.Validate(model).FormatErrors();
                context.Location().LogError("File {file} has validation errors, errors={errors}", file,errors);
                return;
            }

            StatusCode statusCode = await _client.Data.Set(_dataType.GetKey(model), model, context);
            context.Location().Log(statusCode.IsOk() ? LogLevel.Information : LogLevel.Error, "Set file={file}, statusCode={statusCode}", file, statusCode);

        }, file);

        return cmd;
    }

    private Command GetCommand()
    {
        var cmd = new Command("get", $"Get {_dataType.Name}");
        Argument<string> idArgument = new Argument<string>("objectId", $"ObjectId of the {_dataType.Name}, syntax={ObjectId.Syntax}");

        cmd.AddArgument(idArgument);

        cmd.SetHandler(async (objectId) =>
        {
            var context = new ScopeContext(_logger);

            if (!ObjectId.IsValid(objectId))
            {
                _logger.LogError("Invalid objectId={objectId}, syntax={syntax}", objectId, ObjectId.Syntax);
                return;
            }

            Toolbox.Types.Option<T> user = await _client.Data.Get<T>(objectId.ToObjectId(), context);

            context.Location().LogInformation("Get objectId={objectId}, statusCode={statusCode}, mode={model}",
                objectId, user.StatusCode, user.Return().ToJsonSafe(context.Location()));

        }, idArgument);
        return cmd;
    }

    private Command DeleteCommand()
    {
        var cmd = new Command("delete", $"Delete {_dataType.Name}");
        Argument<string> idArgument = new Argument<string>("objectId", $"ObjectId of the {_dataType.Name}, syntax={ObjectId.Syntax}");

        cmd.AddArgument(idArgument);

        cmd.SetHandler(async (objectId) =>
        {
            var context = new ScopeContext(_logger);

            if (!ObjectId.IsValid(objectId))
            {
                _logger.LogError("Invalid objectId={objectId}, syntax={syntax}", objectId, ObjectId.Syntax);
                return;
            }

            Toolbox.Types.Option<T> user = await _client.Data.Get<T>(objectId.ToObjectId(), context);

            context.Location().LogInformation("Get objectId={objectId}, statusCode={statusCode}, mode={model}",
                objectId, user.StatusCode, user.Return().ToJsonSafe(context.Location()));

        }, idArgument);
        return cmd;
    }
}
