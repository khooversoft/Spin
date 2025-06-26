using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataPipelineConfig
{
    string PipelineName { get; }
    TimeSpan? MemoryCacheDuration { get; }
    TimeSpan? FileCacheDuration { get; }
    public string BasePath { get; }
}

public class DataPipelineBuilder : IDataPipelineConfig
{
    public DataPipelineBuilder(IServiceCollection services, string pipelineName)
    {
        Services = services.NotNull();
        PipelineName = pipelineName.NotEmpty();
    }

    public IServiceCollection Services { get; }
    public string PipelineName { get; } = null!;
    public TimeSpan? MemoryCacheDuration { get; set; }
    public TimeSpan? FileCacheDuration { get; set; }
    public string BasePath { get; set; } = null!;
    public DateTime? Date { get; init; }
    public DataPipelineHandlerCollection Handlers { get; } = new();

    public static IValidator<DataPipelineBuilder> Validator { get; } = new Validator<DataPipelineBuilder>()
        .RuleFor(x => x.Services).NotNull()
        .RuleFor(x => x.PipelineName).NotEmpty()
        .RuleFor(x => x.MemoryCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.FileCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.Date).ValidDateTimeOption()
        .RuleFor(x => x.BasePath).NotEmpty()
        .Build();
}


public class DataPipelineHandlerCollection
{
    private readonly IList<Func<IServiceProvider, IDataProvider>> _handlers = new List<Func<IServiceProvider, IDataProvider>>();

    public void Add(Func<IServiceProvider, IDataProvider> handler) => _handlers.Add(handler.NotNull());
    public void Add<T>() where T : class, IDataProvider => _handlers.Add(service => service.GetRequiredService<T>());

    internal Option<IDataProvider> BuildHandlers(IServiceProvider serviceProvider)
    {
        var result = _handlers
            .Reverse()
            .Aggregate((IDataProvider?)null, (prev, current) =>
            {
                var handler = current(serviceProvider);
                handler.InnerHandler = prev;
                return handler;
            });

        return result switch
        {
            null => StatusCode.NotFound,
            _ => result.ToOption<IDataProvider>(),
        };
    }
}

public static class DataPipelineBuilderTool
{
    public static Option Validate(this DataPipelineBuilder subject) => DataPipelineBuilder.Validator.Validate(subject).ToOptionStatus();

    public static string CreatePath<T>(this IDataPipelineConfig config, params string[] segments)
    {
        config.NotNull();
        segments
            .Assert(x => x.Length > 0, "At least one segment is required")
            .Assert(x => x.All(y => y.IsNotEmpty()), "All segments must be non-empty");

        string[] prefix = [config.BasePath, config.PipelineName, typeof(T).Name];

        var list = prefix
            .Concat(segments)
            .Select(x => ToSafePathVector(x.NotNull()))
            .ToArray();

        var path = list
            .Take(list.Length - 1)
            .Append(CreateFileName(list))
            .Join('/').ToLower();

        return path;
    }

    public static string CreateKeyedName<T>(string pipelineName) => $"{pipelineName.NotEmpty()}/{typeof(T).Name}";

    public static DataPipelineBuilder GetDataPipelineBuilder<T>(this IServiceProvider serviceProvider, string pipelineName)
    {
        serviceProvider.NotNull();
        string keyedName = DataPipelineBuilderTool.CreateKeyedName<T>(pipelineName);

        serviceProvider.GetKeyedServices<DataPipelineBuilder>(keyedName)
            .Assert(x => x.Count() == 1, $"Pipeline '{pipelineName}' not found or multiple instances exist");

        DataPipelineBuilder dataContext = serviceProvider.GetRequiredKeyedService<DataPipelineBuilder>(keyedName);
        return dataContext;
    }

    private static string CreateFileName(IEnumerable<string> segments) => segments
        .Select(x => ToSafePathVector(x))
        .Join("__")
        .Func(x => x.NotEmpty() + ".json");

    private static string ToSafePathVector(string path) => path.NotEmpty()
        .Select(x => ToStandardCharacter(x))
        .Func(x => new string([.. x]));

    private static char ToStandardCharacter(char x) => StandardCharacterTest(x) switch
    {
        true => x,
        false => '_',
    };

    private static bool StandardCharacterTest(char x) => char.IsLetterOrDigit(x) || x == '-' || x == '.';
}
