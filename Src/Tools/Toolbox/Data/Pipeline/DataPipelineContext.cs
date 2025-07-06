using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class DataPipelineCommand
{
    public const string Append = "append";
    public const string Delete = "delete";
    public const string Drain = "drain";
    public const string Get = "get";
    public const string Set = "set";

    public const string AppendList = "append-list";
    public const string GetList = "get-list";
    public const string DeleteList = "delete-list";
}

public record DataPipelineContext
{
    public DataPipelineContext(string command, string path, IDataPipelineConfig pipelineConfig)
    {
        Command = command.NotEmpty();
        Path = path.NotEmpty();
        PipelineConfig = pipelineConfig.NotNull();
    }

    public string Command { get; }
    public string Path { get; }
    public bool Queued { get; init; } = false;
    public IDataPipelineConfig PipelineConfig { get; init; } = null!;
    public IReadOnlyList<DataETag> SetData { get; init; } = Array.Empty<DataETag>();
    public IReadOnlyList<DataETag> GetData { get; init; } = Array.Empty<DataETag>();

    public static IValidator<DataPipelineContext> Validator { get; } = new Validator<DataPipelineContext>()
        .RuleFor(x => x.Command).NotEmpty()
        .RuleFor(x => x.Path).NotEmpty()
        .RuleFor(x => x.PipelineConfig).NotNull()
        .RuleFor(x => x.SetData).NotNull()
        .RuleFor(x => x.SetData).NotNull()
        .Build();
}

public static class DataPipelineContextExtensions
{
    public static Option Validate(this DataPipelineContext subject) => DataPipelineContext.Validator.Validate(subject).ToOptionStatus();

    public static DataPipelineContext CreateAppend<T>(this IDataPipelineConfig pipelineConfig, string key, DataETag data)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Append, path, pipelineConfig) { SetData = [data] };
    }

    public static DataPipelineContext CreateDelete<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Delete, path, pipelineConfig);
    }

    public static DataPipelineContext CreateDrain(this IDataPipelineConfig pipelineConfig)
    {
        return new DataPipelineContext(DataPipelineCommand.Drain, "cmd:drain", pipelineConfig);
    }

    public static DataPipelineContext CreateGet<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Get, path, pipelineConfig);
    }

    public static DataPipelineContext CreateSet<T>(this IDataPipelineConfig pipelineConfig, string key, DataETag data)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Set, path, pipelineConfig) { SetData = [data] };
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// List Operations
    /// 

    public static DataPipelineContext CreateAppendList<T>(this IDataPipelineConfig pipelineConfig, string key, params IEnumerable<DataETag> data)
    {
        data.NotNull().Assert(x => x.Any(), "Data cannot be empty");

        var config = pipelineConfig.CreateConfig<T>(key);
        string fullPath = pipelineConfig.NotNull().ListPartitionStrategy(config);
        string path = pipelineConfig.CreatePath<T>(fullPath);

        return new DataPipelineContext(DataPipelineCommand.AppendList, path, pipelineConfig) { SetData = [.. data] };
    }

    public static DataPipelineContext CreateDeleteList<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateSearchList<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.DeleteList, path, pipelineConfig);
    }

    public static DataPipelineContext CreateGetList<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateSearchList<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.GetList, path, pipelineConfig);
    }


    private static string InternalCreateFilePath<T>(IDataPipelineConfig pipelineConfig, string key)
    {
        var config = pipelineConfig.CreateConfig<T>(key);
        string fullPath = pipelineConfig.NotNull().FilePartitionStrategy(config);
        string path = pipelineConfig.CreatePath<T>(fullPath);
        return path;
    }

    private static string InternalCreateSearchList<T>(IDataPipelineConfig pipelineConfig, string key)
    {
        var config = pipelineConfig.CreateConfig<T>(key);
        string fullPath = pipelineConfig.NotNull().ListPartitionSearch(config);
        string path = pipelineConfig.CreatePath<T>(fullPath);
        return path;
    }
}
