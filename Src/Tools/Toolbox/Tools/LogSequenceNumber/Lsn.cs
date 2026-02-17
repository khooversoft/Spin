using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Toolbox.Tools;

[DebuggerDisplay("Timestamp = {Timestamp}, Counter = {Counter}, TimestampDate={TimestampDate}")]
public readonly struct Lsn : IEquatable<Lsn>
{
    public static readonly Lsn Default = new(0, 0);

    public Lsn(long timestamp, long counter) => (Timestamp, Counter) = (timestamp, counter);

    public Lsn(DateTime timestamp, long counter)
    {
        Timestamp = new DateTimeOffset(timestamp, TimeSpan.Zero).ToUnixTimeMilliseconds();
        Counter = counter;
        TimestampDate = DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).UtcDateTime;
    }

    public long Timestamp { get; }
    public long Counter { get; }
    public DateTime TimestampDate { get; }

    public bool Equals(Lsn other) => Timestamp == other.Timestamp && Counter == other.Counter;
    public override bool Equals(object? obj) => obj is Lsn other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Timestamp, Counter);

    public static bool operator ==(Lsn left, Lsn right) => left.Equals(right);
    public static bool operator !=(Lsn left, Lsn right) => !left.Equals(right);

    public static bool operator <(Lsn left, Lsn right) =>
        left.Timestamp < right.Timestamp ||
        (left.Timestamp == right.Timestamp && left.Counter < right.Counter);

    public static bool operator >(Lsn left, Lsn right) =>
        left.Timestamp > right.Timestamp ||
        (left.Timestamp == right.Timestamp && left.Counter > right.Counter);

    public static bool operator <=(Lsn left, Lsn right) =>
        left < right || left == right;

    public static bool operator >=(Lsn left, Lsn right) =>
        left > right || left == right;

    public static Lsn Parse(string sequenceNumber) => LsnTool.Parse(sequenceNumber);
}

public static partial class LsnTool
{
    public static Lsn Parse(string seqNumber)
    {
        seqNumber.NotEmpty();

        Match match = KeyWithRegex().Match(seqNumber);
        if (!match.Success) throw new ArgumentException("Invalid log sequence number format", nameof(seqNumber));

        if (!long.TryParse(match.Groups["timestamp"].Value, out long timestamp))
        {
            throw new ArgumentException("Invalid log sequence number format", nameof(seqNumber));
        }

        if (!long.TryParse(match.Groups["counter"].Value, out long counter))
        {
            throw new ArgumentException("Invalid log sequence number format", nameof(seqNumber));
        }

        return new Lsn(timestamp, counter);
    }

    // Matches LSN format: {timestampMillis:D15}-{counter:D6}-{randomHex:4 alphanumeric}
    [GeneratedRegex(@"^(?<timestamp>\d{15})-(?<counter>\d{6})-(?<randomHex>[A-Za-z0-9]{4})$", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex KeyWithRegex();

}