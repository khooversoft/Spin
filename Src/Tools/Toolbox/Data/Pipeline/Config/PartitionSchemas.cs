using System.Security.Cryptography;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;

public static class PartitionSchemas
{
    public static string ScalarFile(IDataPipelineConfig config, string type, string key)
    {
        config.NotNull().PipelineName.NotEmpty();
        type.NotEmpty();
        key.NotEmpty();

        return $"{config.PipelineName}/{type}/{key}-{type}";
    }

    public static string DailyListPartitioning(IDataPipelineConfig config, string type, string key)
    {
        config.NotNull().PipelineName.NotEmpty();
        type.NotEmpty();
        key.NotEmpty();

        DateTime now = DateTime.UtcNow;

        return $"{config.PipelineName}/{type}/{now:yyyyMM}/{key}-{now:yyyyMMdd}-{type}";
    }

    public static string DailyListPartitionSearch(IDataPipelineConfig config, string type, string key)
    {
        config.NotNull().PipelineName.NotEmpty();
        type.NotEmpty();
        key.NotEmpty();

        return $"{config.PipelineName}/{type}/**/{key}-*-{type}*";
    }

    public static string BatchPartitioning(IDataPipelineConfig config, string type, string key)
    {
        config.NotNull().PipelineName.NotEmpty();
        type.NotEmpty();
        key.NotEmpty();

        DateTime now = DateTime.UtcNow;
        string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

        return $"{config.PipelineName}/{type}/{now:yyyyMM}/{key}-{now:yyyyMMdd}-{randString}-{type}";
    }
}
