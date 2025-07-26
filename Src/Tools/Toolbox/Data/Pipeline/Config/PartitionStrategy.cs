using System.Security.Cryptography;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Data;


public record PartitionStrategy
{
    public Func<PathDetail, string> File { get; init; } = PartitionSchemas.ScalarFile;
    public Func<PathDetail, string> List { get; init; } = PartitionSchemas.DailyListPartitioning;
    public Func<PathDetail, string> Search { get; init; } = PartitionSchemas.DailyListPartitionSearch;
}

public static class PartitionSchemas
{
    public static string ScalarFile(PathDetail fileDetail) => fileDetail.TypeName switch
    {
        null => $"{fileDetail.NotNull().PipelineName}/{fileDetail.Key}",
        not null => $"{fileDetail.NotNull().PipelineName}/{fileDetail.TypeName}/{fileDetail.Key}-{fileDetail.TypeName}"
    };

    public static string DailyListPartitioning(PathDetail fileDetail)
    {
        fileDetail.NotNull();
        DateTime now = DateTime.UtcNow;

        return fileDetail.TypeName switch
        {
            not null => $"{fileDetail.PipelineName}/{fileDetail.TypeName}/{now:yyyyMM}/{fileDetail.Key}-{now:yyyyMMdd}-{fileDetail.TypeName}",
            null => $"{fileDetail.PipelineName}/{now:yyyyMM}/{fileDetail.Key}-{now:yyyyMMdd}-{fileDetail.TypeName}"
        };
    }

    public static string DailyListPartitionSearch(PathDetail fileDetail) => fileDetail.TypeName switch
    {
        not null => $"{fileDetail.NotNull().PipelineName}/{fileDetail.TypeName}/**/{fileDetail.Key}-*-{fileDetail.TypeName}*",
        null => $"{fileDetail.NotNull().PipelineName}/**/{fileDetail.Key}-*-{fileDetail.TypeName}*",
    };

    public static string BatchPartitioning(PathDetail fileDetail)
    {
        fileDetail.NotNull();

        DateTime now = DateTime.UtcNow;
        string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

        return fileDetail.TypeName switch
        {
            not null => $"{fileDetail.PipelineName}/{fileDetail.TypeName}/{now:yyyyMM}/{fileDetail.Key}-{now:yyyyMMdd}-{randString}-{fileDetail.TypeName}",
            null => $"{fileDetail.PipelineName}/{now:yyyyMM}/{fileDetail.Key}-{now:yyyyMMdd}-{randString}-{fileDetail.TypeName}",
        };
    }
}
