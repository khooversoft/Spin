using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace SpinCluster.sdk.Services;

public record DatalakeEndpoint
{
    public string Account { get; init; } = null!;
    public string Container { get; init; } = null!;
    public string Path { get; init; } = null!;

    public static DatalakeEndpoint Create(string connectionString)
    {
        var values = connectionString.NotEmpty()
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => parseProperty(x))
            .OfType<KeyValuePair<string, string?>>()
            .ToArray();

        var result = values.ToObject<DatalakeEndpoint>();
        return result;

        KeyValuePair<string, string>? parseProperty(string property) => property.Split('=', StringSplitOptions.RemoveEmptyEntries) switch
        {
            var v when v.Length != 2 => null,
            var v => new KeyValuePair<string, string>(v[0], v[1])
        };
    }

    public static Validator<DatalakeEndpoint> Validator { get; } = new Validator<DatalakeEndpoint>()
        .RuleFor(x => x.Account).NotEmpty()
        .RuleFor(x => x.Container).NotEmpty()
        .RuleFor(x => x.Path).NotEmpty()
        .Build();
}

public static class DatalakeLocationValidator
{
    public static Option Validate(this DatalakeEndpoint subject) => DatalakeEndpoint.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this DatalakeEndpoint subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}