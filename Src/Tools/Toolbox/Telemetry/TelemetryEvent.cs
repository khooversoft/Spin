using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Telemetry;


public record TelemetryEvent
{
    public string Id { get; } = Guid.NewGuid().ToString();
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public string Name { get; init; } = null!;                  // Event name
    public string EventType { get; init; } = null!;             // Event type name, Counter, Gauge, Histogram
    public string Value { get; init; } = null!;                 // Current value based on type
    public string ValueType { get; init; } = null!;             // Value's Type name
    public string? Scope { get; init; }                         // Scope name
    public string? Description { get; init; }                   // Metric description
    public string? Version { get; init; }                       // Metric version
    public string? Tags { get; init; }                          // Tags merged from definition and event
    public string? Units { get; init; }                         // Definition units

    public static IValidator<TelemetryEvent> Validator { get; } = new Validator<TelemetryEvent>()
        .RuleFor(x => x.Id).NotEmpty()
        .RuleFor(x => x.Name).NotEmpty()
        .RuleFor(x => x.EventType).NotEmpty()
        .RuleFor(x => x.Value).NotEmpty()
        .RuleFor(x => x.ValueType).NotEmpty()
        .Build();
}


public static class TelemetryEventExtensions
{
    public static Option Validate(this TelemetryEvent subject) => TelemetryEvent.Validator.Validate(subject).ToOptionStatus();

    public static bool TryGet<T>(this TelemetryEvent subject, out T? result)
    {
        subject.NotNull();

        switch (typeof(T).Name)
        {
            case "Int32":
                result = (T)(object)int.Parse(subject.Value);
                return true;

            case "Int64":
                result = (T)(object)long.Parse(subject.Value);
                return true;

            default: throw new NotSupportedException($"Type {typeof(T).Name} is not supported");
        }
    }
}