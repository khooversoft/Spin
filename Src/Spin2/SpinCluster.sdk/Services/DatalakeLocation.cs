using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Tools.Validation;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

public record DatalakeLocation
{
    public string Account { get; init; } = null!;
    public string Container { get; init; } = null!;
    public string Path { get; init; } = null!;

    public static Option<DatalakeLocation> ParseConnectionString(string connectionString)
    {
        var values = connectionString.NotEmpty()
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => parseProperty(x))
            .OfType<KeyValuePair<string, string>>()
            .ToArray();

        var result = values.ToObject<DatalakeLocation>();
        return result;

        KeyValuePair<string, string>? parseProperty(string property)
        {
            return property.Split('=', StringSplitOptions.RemoveEmptyEntries) switch
            {
                var v when v.Length != 2 => default,
                var v => new KeyValuePair<string, string>(v[0], v[1])
            };
        }
    }
}


public static class DatalakeLocationValidator
{
    public static Validator<DatalakeLocation> Validator { get; } = new Validator<DatalakeLocation>()
        .RuleFor(x => x.Account).NotEmpty()
        .RuleFor(x => x.Container).NotEmpty()
        .RuleFor(x => x.Path).NotEmpty()
        .Build();

    public static Option Validate(this DatalakeLocation subject) => Validator.Validate(subject).ToOptionStatus();
}