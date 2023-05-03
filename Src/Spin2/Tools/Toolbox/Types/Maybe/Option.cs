namespace Toolbox.Types.Maybe;

public readonly struct Option : IEquatable<Option>
{
    public Option() => StatusCode = OptionStatus.OK;
    public Option(OptionStatus optionStatus)
    {
        StatusCode = optionStatus;
    }

    public OptionStatus StatusCode { get; }

    public override int GetHashCode() => HashCode.Combine(StatusCode);
    public override bool Equals(object? obj) => obj is Option option && option.StatusCode == StatusCode;
    public bool Equals(Option other) => StatusCode == other.StatusCode;

    public static bool operator ==(Option left, Option right) => left.Equals(right);
    public static bool operator !=(Option left, Option right) => !(left == right);

    public static implicit operator OptionStatus(Option other) => other.StatusCode;
    public static implicit operator Option(OptionStatus value) => new Option(value);
}


public static class OptionExtensions
{
    public static Option ToOption(this OptionStatus option) => new Option(option);
}