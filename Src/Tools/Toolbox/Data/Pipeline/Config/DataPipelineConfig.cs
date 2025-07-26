using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataPipelineConfig
{
    string PipelineName { get; }
    string ServiceKeyedName { get; }
    TimeSpan? MemoryCacheDuration { get; }
    TimeSpan? FileCacheDuration { get; }
    string BasePath { get; }
    IReadOnlyDictionary<string, string?>? Tags { get; }
    PartitionStrategy PartitionStrategy { get; set; }
}

public class DataPipelineConfig : IDataPipelineConfig
{
    public DataPipelineConfig(IServiceCollection services, string pipelineName, string serviceKeyedName)
    {
        Services = services.NotNull();
        PipelineName = pipelineName.NotEmpty();
        ServiceKeyedName = serviceKeyedName;
    }

    public IServiceCollection Services { get; }
    public string PipelineName { get; } = null!;
    public string ServiceKeyedName { get; }
    public TimeSpan? MemoryCacheDuration { get; set; }
    public TimeSpan? FileCacheDuration { get; set; }
    public string BasePath { get; set; } = null!;
    public DateTime? Date { get; init; }
    public IReadOnlyDictionary<string, string?>? Tags { get; set; }
    public PartitionStrategy PartitionStrategy { get; set; } = new();
    public DataPipelineHandlerCollection Handlers { get; } = new();

    public static IValidator<DataPipelineConfig> Validator { get; } = new Validator<DataPipelineConfig>()
        .RuleFor(x => x.Services).NotNull()
        .RuleFor(x => x.PipelineName).NotEmpty()
        .RuleFor(x => x.ServiceKeyedName).NotEmpty()
        .RuleFor(x => x.MemoryCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.FileCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.BasePath).NotEmpty()
        .RuleFor(x => x.Date).ValidDateTimeOption()
        .Build();
}

public static class DataPipelineConfigTool
{
    public static Option Validate(this DataPipelineConfig subject) => DataPipelineConfig.Validator.Validate(subject).ToOptionStatus();

    public static PathDetail CreateConfig<T>(this IDataPipelineConfig pipelineConfig, string key) => new PathDetail
    {
        PipelineName = pipelineConfig.PipelineName.NotEmpty(),
        TypeName = typeof(T).Name.Func(x => x == typeof(DataETag).Name ? null : x),
        Key = key.NotEmpty(),
    };

    public static string CreatePath<T>(this IDataPipelineConfig config, string filePath)
    {
        config.NotNull();
        filePath.NotEmpty();

        filePath = filePath.EndsWith(".json") ? filePath : filePath += ".json";

        var segments = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();

        string[] fullSegments = [.. config.BasePath.Split('/'), .. segments];

        var path = fullSegments
            .Select(x => ToSafePathVector(x.NotNull()))
            .Join('/').NotEmpty()
            .ToLower();

        return path;
    }

    public static string CreateKeyedName<T>(string pipelineName) => $"{pipelineName.NotEmpty()}/{typeof(T).Name}";

    public static DataPipelineConfig GetDataPipelineBuilder<T>(this IServiceProvider serviceProvider, string pipelineName)
    {
        serviceProvider.NotNull();
        string keyedName = DataPipelineConfigTool.CreateKeyedName<T>(pipelineName);

        serviceProvider.GetKeyedServices<DataPipelineConfig>(keyedName)
            .Assert(x => x.Count() == 1, $"Pipeline '{pipelineName}' not found or multiple instances exist");

        DataPipelineConfig dataContext = serviceProvider.GetRequiredKeyedService<DataPipelineConfig>(keyedName);
        dataContext.NotNull().Validate().ThrowOnError();
        dataContext.ServiceKeyedName.Be(keyedName);

        return dataContext;
    }

    private static string ToSafePathVector(string path) => path.NotEmpty()
        .Select(x => ToStandardCharacter(x))
        .Func(x => new string([.. x]));

    private static char ToStandardCharacter(char x) => StandardCharacterTest(x) switch
    {
        true => x,
        false => '_',
    };

    private static bool StandardCharacterTest(char x) => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '*';
}