using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Store;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public interface IDataPipelineConfig
{
    TimeSpan? MemoryCacheDuration { get; }
    TimeSpan? FileCacheDuration { get; }
    string BasePath { get; }
    IReadOnlyDictionary<string, string?>? Tags { get; }
    //IFileSystem FileSystem { get; set; }
}

public interface IDataPipelineBuilder
{
    IServiceCollection Services { get; }
    DataPipelineHandlerCollection Handlers { get; }

    TimeSpan? MemoryCacheDuration { get; set; }
    TimeSpan? FileCacheDuration { get; set; }
    string BasePath { get; set; }
    IReadOnlyDictionary<string, string?>? Tags { get; set; }
    //IFileSystem FileSystem { get; set; }
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
    //public IFileSystem FileSystem { get; set; } = null!;

    public static IValidator<DataPipelineConfig<T>> Validator { get; } = new Validator<DataPipelineConfig<T>>()
        .RuleFor(x => x.Services).NotNull()
        .RuleFor(x => x.MemoryCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.FileCacheDuration).ValidTimeSpanOption()
        .RuleFor(x => x.BasePath).NotEmpty()
        //.RuleFor(x => x.FileSystem).NotNull()
        .Build();
}


public static class DataPipelineConfigTool
{
    public static Option Validate<T>(this DataPipelineConfig<T> subject) => DataPipelineConfig<T>.Validator.Validate(subject).ToOptionStatus();

    public static string CreatePath<T>(this IDataPipelineConfig config, string key)
    {
        config.NotNull();
        key.NotEmpty();

        //string path = config.FileSystem.PathBuilder<T>(key);
        //path = config.CreateSafePath(path);
        //return path;
        return default!;
    }

    public static string CreateSearch(this IDataPipelineConfig config, string? key, string? pattern)
    {
        config.NotNull();
        key.NotEmpty();

        string path = (key, pattern) switch
        {
            (null, null) => "**/*",
            (string k, null) => $"{k}/**/*",
            (null, string p) => p,
            (string k, string p) => $"{k}/{p}",
        };

        path = config.CreateSafePath(path);
        return path;
    }

    private static string CreateSafePath(this IDataPipelineConfig config, string key)
    {
        config.NotNull();
        key.NotEmpty();

        var segments = key.Split('/', StringSplitOptions.RemoveEmptyEntries).ToArray();

        string[] fullSegments = [.. config.BasePath.Split('/'), .. segments];

        var fullPath = fullSegments
            .Select(x => ToSafePathVector(x.NotNull()))
            .Join('/').NotEmpty()
            .ToLower();

        return fullPath;
    }

    private static string ToSafePathVector(string path) => path.NotEmpty()
        .Select(x => ToStandardCharacter(x))
        .Func(x => new string([.. x]));

    private static char ToStandardCharacter(char x) => StandardCharacterTest(x) switch
    {
        true => x,
        false => '_',
    };

    private static bool StandardCharacterTest(char x) => char.IsLetterOrDigit(x) || x == '-' || x == '.' || x == '*' || x == '_';
}