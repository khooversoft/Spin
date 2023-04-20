namespace Toolbox.Types;

/// <summary>
/// Unix date is number of seconds from 1970-01-01T00:00:00Z
/// </summary>
public struct UnixDate : IEquatable<UnixDate>
{
    public UnixDate(long timeStamp) => TimeStamp = timeStamp;

    public UnixDate(DateTimeOffset utcDate) => TimeStamp = utcDate.ToUnixTimeSeconds();

    public UnixDate(DateTime utcDate) => TimeStamp = ((DateTimeOffset)utcDate).ToUnixTimeSeconds();

    public static UnixDate UtcNow => (UnixDate)DateTimeOffset.UtcNow;

    public long TimeStamp { get; }

    public DateTimeOffset DateTimeOffset => DateTimeOffset.FromUnixTimeSeconds(TimeStamp);

    public DateTime DateTime => DateTimeOffset.UtcDateTime;


    public static explicit operator UnixDate(long timeStamp) => new UnixDate(timeStamp);

    public static explicit operator UnixDate(DateTimeOffset dateTimeOffset) => new UnixDate(dateTimeOffset);

    public static explicit operator UnixDate(DateTime datetime) => new UnixDate(datetime);

    public static implicit operator DateTimeOffset(UnixDate unixDate) => DateTimeOffset.FromUnixTimeSeconds(unixDate);

    public static implicit operator long(UnixDate unix) => unix.TimeStamp;

    public static bool operator ==(UnixDate left, UnixDate right) => left.Equals(right);

    public static bool operator !=(UnixDate left, UnixDate right) => !(left == right);

    public override bool Equals(object? obj) => obj is UnixDate date && Equals(date);

    public bool Equals(UnixDate other) => TimeStamp == other.TimeStamp;

    public override int GetHashCode() => HashCode.Combine(TimeStamp);
}


public static class UnixDateExtensions
{
    public static UnixDate ToUnixDate(this long value) => new UnixDate(value);
    public static UnixDate ToUnixDate(this DateTime value) => new UnixDate(value);
    public static UnixDate ToUnixDate(this DateTimeOffset value) => new UnixDate(value);
    public static DateTime ToDateTime(this long value) => new UnixDate(value).DateTime;
}
