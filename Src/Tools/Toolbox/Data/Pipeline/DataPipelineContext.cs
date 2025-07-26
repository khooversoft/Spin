using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class DataPipelineCommand
{
    public const string Append = "append";
    public const string Delete = "delete";
    public const string Get = "get";
    public const string Set = "set";

    public const string AppendList = "append-list";
    public const string GetList = "get-list";
    public const string DeleteList = "delete-list";

    public const string Drain = "drain";
    public const string AcquireLock = "lock-acquire";
    public const string AcquireExclusiveLock = "lock-exclusive";
    public const string ReleaseLock = "lock-release";
}

public record DataPipelineContext
{
    public DataPipelineContext(string command, string path, IDataPipelineConfig pipelineConfig)
    {
        Command = command.NotEmpty();
        Path = path.NotEmpty();
        PipelineConfig = pipelineConfig.NotNull();
    }

    public DataPipelineContext(string command, string path, PathDetail pathDetail, IDataPipelineConfig pipelineConfig)
    {
        Command = command.NotEmpty();
        Path = path.NotEmpty();
        PathDetail = pathDetail.NotNull().Action(x => x.Validate().ThrowOnError());
        PipelineConfig = pipelineConfig.NotNull();
    }

    public string Command { get; }
    public string Path { get; }
    public PathDetail? PathDetail { get; }
    public IDataPipelineConfig PipelineConfig { get; }
    public IReadOnlyList<DataETag> SetData { get; init; } = Array.Empty<DataETag>();
    public IReadOnlyList<DataETag> GetData { get; init; } = Array.Empty<DataETag>();

    public static IValidator<DataPipelineContext> Validator { get; } = new Validator<DataPipelineContext>()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.PathDetail).ValidateOption(PathDetail.Validator)
        .RuleFor(x => x.PipelineConfig).NotNull()
        .RuleFor(x => x.SetData).NotNull()
        .RuleFor(x => x.GetData).NotNull()
        .Build();
}

public static class DataPipelineContextExtensions
{
    public static Option Validate(this DataPipelineContext subject) => DataPipelineContext.Validator.Validate(subject).ToOptionStatus();
}

