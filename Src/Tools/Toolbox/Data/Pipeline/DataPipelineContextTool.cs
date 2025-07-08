using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class DataPipelineContextTool
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


    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Operations
    /// 

    public static DataPipelineContext CreateDrain(this IDataPipelineConfig pipelineConfig)
    {
        return new DataPipelineContext(DataPipelineCommand.Drain, "cmd:drain", pipelineConfig);
    }

    public static DataPipelineContext AcquireLock<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.AcquireLock, path, pipelineConfig);
    }

    public static DataPipelineContext AcquireExclusiveLock<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.AcquireExclusiveLock, path, pipelineConfig);
    }

    public static DataPipelineContext CreateReleaseLock<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        string path = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.ReleaseLock, path, pipelineConfig);
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Support
    /// 

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
