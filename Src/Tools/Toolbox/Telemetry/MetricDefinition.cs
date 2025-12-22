using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Telemetry;

public record MetricDefinition
{
    public const string CounterType = "counter";
    public const string GaugeType = "gauge";
    public const string HistogramType = "histogram";

    public string Name { get; init; } = null!;      // {sourceName}.{metricName}
    public string Type { get; init; } = null!;      // CounterType, GaugeType, HistogramType
    public string? Tags { get; init; }
    public string? Version { get; init; }
    public string? Description { get; init; }
    public string? Unit { get; init; }

    public static IValidator<MetricDefinition> Validator { get; } = new Validator<MetricDefinition>()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.Type).Must(x => x == CounterType || x == GaugeType || x == HistogramType, _ => $"Type must be one of: {CounterType}, {GaugeType}, {HistogramType}")
        .RuleFor(x => x.Tags).Must(x => x == null || PropertyStringSchema.Tags.Parse(x).IsOk(), _ => "Tags must be in valid format")
        .Build();
}

public static class MetricDefinitionExtensions
{
    public static Option Validate(this MetricDefinition subject) => MetricDefinition.Validator.Validate(subject).ToOptionStatus();
}