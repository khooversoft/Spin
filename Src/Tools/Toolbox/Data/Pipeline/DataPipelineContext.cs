using System.Collections.Immutable;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class DataPipelineCommand
{
    public const string Append = "append";
    public const string Delete = "delete";
    public const string Get = "get";
    public const string Set = "set";
    public const string Search = "search";

    public const string AppendList = "append-list";
    public const string GetList = "get-list";
    public const string DeleteList = "delete-list";
    public const string SearchList = "search-list";

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

    public DataPipelineContext(string command, string path, IEnumerable<DataETag> data, IDataPipelineConfig pipelineConfig)
        : this(command, path, pipelineConfig)
    {
        data.NotNull().Assert(x => x.Count() > 0, _ => "Data list cannot be empty");
        SetData = data.ToImmutableArray();
    }

    public string Command { get; }
    public string Path { get; }
    public string? Pattern { get; init; }
    public IDataPipelineConfig PipelineConfig { get; }
    public IReadOnlyList<DataETag> SetData { get; set; } = Array.Empty<DataETag>();
    public IReadOnlyList<DataETag> GetData { get; set; } = Array.Empty<DataETag>();

    public static IValidator<DataPipelineContext> Validator { get; } = new Validator<DataPipelineContext>()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.PipelineConfig).NotNull()
        .RuleFor(x => x.SetData).NotNull()
        .RuleFor(x => x.GetData).NotNull()
        .Build();
}

public static class DataPipelineContextExtensions
{
    public static Option Validate(this DataPipelineContext subject) => DataPipelineContext.Validator.Validate(subject).ToOptionStatus();
}

