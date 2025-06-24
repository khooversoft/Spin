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

    public const string AppendList = "append-list";
    public const string GetList = "get-list";
}

public record DataPipelineContext
{
    public string Command { get; init; } = null!;
    public string Key { get; init; } = null!;
    public IReadOnlyList<DataETag> SetData { get; init; } = Array.Empty<DataETag>();
    public IReadOnlyList<DataETag> GetData { get; init; } = Array.Empty<DataETag>();
}

public record DataAppend : DataPipelineContext
{
    public DataAppend(string key, DataETag setData)
    {
        Command = DataPipelineCommand.Append;
        Key = key.NotEmpty();
        SetData = [setData];
    }
}

public record DataDelete : DataPipelineContext
{
    public DataDelete(string key)
    {
        Command = DataPipelineCommand.Delete;
        Key = key.NotEmpty();
    }
}

public record DataGet : DataPipelineContext
{
    public DataGet(string key)
    {
        Command = DataPipelineCommand.Get;
        Key = key.NotEmpty();
    }
}

public record DataSet : DataPipelineContext
{
    public DataSet(string key, DataETag setData)
    {
        Command = DataPipelineCommand.Set;
        Key = key.NotEmpty();
        SetData = [setData];
    }
}

public record DataAppendList : DataPipelineContext
{
    public DataAppendList(string key, IEnumerable<DataETag> dataItems)
    {
        Command = DataPipelineCommand.AppendList;
        Key = key.NotEmpty();
        SetData = dataItems.NotNull().ToImmutableArray();
    }
}

public record DataGetList : DataPipelineContext
{
    public DataGetList(string key)
    {
        Command = DataPipelineCommand.GetList;
        Key = key.NotEmpty();
    }
}

