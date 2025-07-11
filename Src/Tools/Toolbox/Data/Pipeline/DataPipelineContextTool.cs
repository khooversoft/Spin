using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public static class DataPipelineContextTool
{
    public static Option Validate(this DataPipelineContext subject) => DataPipelineContext.Validator.Validate(subject).ToOptionStatus();

    public static DataPipelineContext CreateAppend<T>(this IDataPipelineConfig pipelineConfig, string key, DataETag data)
    {
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Append, path, pathDetail, pipelineConfig) { SetData = [data] };
    }

    public static DataPipelineContext CreateDelete<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Delete, path, pathDetail, pipelineConfig);
    }

    public static DataPipelineContext CreateGet<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Get, path, pathDetail, pipelineConfig);
    }

    public static DataPipelineContext CreateSet<T>(this IDataPipelineConfig pipelineConfig, string key, DataETag data)
    {
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.Set, path, pathDetail, pipelineConfig) { SetData = [data] };
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// List Operations
    /// 

    public static DataPipelineContext CreateAppendList<T>(this IDataPipelineConfig pipelineConfig, string key, params IEnumerable<DataETag> data)
    {
        data.NotNull().Assert(x => x.Any(), "Data cannot be empty");

        var pathDetail = pipelineConfig.CreateConfig<T>(key);
        string fullPath = pipelineConfig.NotNull().PartitionStrategy.List(pathDetail);
        string path = pipelineConfig.CreatePath<T>(fullPath);

        return new DataPipelineContext(DataPipelineCommand.AppendList, path, pathDetail, pipelineConfig) { SetData = [.. data] };
    }

    public static DataPipelineContext CreateDeleteList<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        (string path, PathDetail pathDetail) = InternalCreateSearchList<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.DeleteList, path, pathDetail, pipelineConfig);
    }

    public static DataPipelineContext CreateGetList<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        (string path, PathDetail pathDetail) = InternalCreateSearchList<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.GetList, path, pathDetail, pipelineConfig);
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
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.AcquireLock, path, pathDetail, pipelineConfig);
    }

    public static DataPipelineContext AcquireExclusiveLock<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.AcquireExclusiveLock, path, pathDetail, pipelineConfig);
    }

    public static DataPipelineContext CreateReleaseLock<T>(this IDataPipelineConfig pipelineConfig, string key)
    {
        (string path, PathDetail pathDetail) = InternalCreateFilePath<T>(pipelineConfig, key);
        return new DataPipelineContext(DataPipelineCommand.ReleaseLock, path, pathDetail, pipelineConfig);
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// Support
    /// 

    private static (string path, PathDetail pathDetail) InternalCreateFilePath<T>(IDataPipelineConfig pipelineConfig, string key)
    {
        var pathDetail = pipelineConfig.CreateConfig<T>(key);
        string fullPath = pipelineConfig.NotNull().PartitionStrategy.File(pathDetail);
        string path = pipelineConfig.CreatePath<T>(fullPath);
        return (path, pathDetail);
    }

    private static (string path, PathDetail pathDetail) InternalCreateSearchList<T>(IDataPipelineConfig pipelineConfig, string key)
    {
        var pathDetail = pipelineConfig.CreateConfig<T>(key);
        string fullPath = pipelineConfig.NotNull().PartitionStrategy.Search(pathDetail);
        string path = pipelineConfig.CreatePath<T>(fullPath);
        return (path, pathDetail);
    }
}
