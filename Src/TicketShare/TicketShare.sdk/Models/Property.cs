using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace TicketShare.sdk;

[GenerateSerializer]
public sealed record Property : IEquatable<Property>
{
    [Id(0)] public string Key { get; init; } = null!;
    [Id(1)] public string? Value { get; init; }

    public bool Equals(Property? other) =>
        other != null &&
        Key.EqualsIgnoreCase(other.Key) &&
        Value.EqualsIgnoreCaseOption(other.Value);

    public override int GetHashCode() => HashCode.Combine(Key, Value);

    public static IValidator<Property> Validator { get; } = new Validator<Property>()
        .RuleFor(x => x.Key).NotEmpty()
        .Build();
}

public static class PropertyExtensions
{
    public static Option Validate(this Property subject) => Property.Validator.Validate(subject).ToOptionStatus();

    public static bool Validate(this Property subject, out Option result)
    {
        result = subject.Validate();
        return result.IsOk();
    }
}