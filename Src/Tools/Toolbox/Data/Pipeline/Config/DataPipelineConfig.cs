using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataPipelineConfig
{
    TimeSpan? MemoryCacheDuration { get; }
    TimeSpan? FileCacheDuration { get; }
    string BasePath { get; }
    IReadOnlyDictionary<string, string?>? Tags { get; }
    Func<string, string> CreatePath { get; set; }
    Func<string, string, string> CreateSearch { get; set; }
}

public interface IDataPipelineBuilder
{
    IServiceCollection Services { get; }
    DataPipelineHandlerCollection Handlers { get; }

    TimeSpan? MemoryCacheDuration { get; set; }
    TimeSpan? FileCacheDuration { get; set; }
    string BasePath { get; set; }
    IReadOnlyDictionary<string, string?>? Tags { get; set; }
    Func<string, string> CreatePath { get; set; }
    Func<string, string, string> CreateSearch { get; set; }
}

public class DataPipelineConfig<T> : IDataPipelineBuilder, IDataPipelineConfig
{
    public DataPipelineConfig(IServiceCollection services) => Services = services.NotNull();

    public IServiceCollection Services { get; }
    public DataPipelineHandlerCollection Handlers { get; } = new();

    public TimeSpan? MemoryCacheDuration { get; set; }
    public TimeSpan? FileCacheDuration { get; set; }
    public string BasePath { get; set; } = null!;
    public IReadOnlyDictionary<string, string?>? Tags { get; set; }
    public Func<string, string> CreatePath { get; set; } = null!;
    public Func<string, string, string> CreateSearch { get; set; } = null!;

    public static IValidator<DataPipelineConfig<T>> Validator { get; } = new Validator<DataPipelineConfig<T>>()
        .RuleFor(x => x.Services).NotNull()
        .RuleFor(x => x.MemoryCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.FileCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.BasePath).NotEmpty()
        .RuleFor(x => x.CreatePath).NotNull()
        .RuleFor(x => x.CreateSearch).NotNull()
        .Build();
}


public static class DataPipelineConfigTool
{
    public static Option Validate<T>(this DataPipelineConfig<T> subject) => DataPipelineConfig<T>.Validator.Validate(subject).ToOptionStatus();

    public static string CreatePath(this IDataPipelineConfig config, string filePath)
    {
        config.NotNull();
        filePath.NotEmpty();

        var segments = filePath.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();

        string[] fullSegments = [.. config.BasePath.Split('/'), .. segments];

        var path = fullSegments
            .Select(x => ToSafePathVector(x.NotNull()))
            .Join('/').NotEmpty()
            .ToLower();

        return path;
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