using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public class LogSequenceNumber
{
    private long _counter = 0;

    public long GetCounter() => _counter;

    public string Next()
    {
        var counter = Interlocked.Increment(ref _counter);

        string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var result = $"{now.ToUnixTimeMilliseconds():D15}-{counter.ToString("D6")}-{randString}";
        return result;
    }

    public static Lsn Parse(string? logSequenceNumber)
    {
        if (logSequenceNumber.IsEmpty()) return Lsn.Default;

        // format = "{timestamp in milliseconds since epoch}-{counter}-{random string}"
        long.TryParse(logSequenceNumber[..15], out long milliseconds).Assert(x => x == true, "Invalid log sequence number format");
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        long.TryParse(logSequenceNumber[16..22], out long counter).Assert(x => x == true, "Invalid log sequence number format");

        return new Lsn(timestamp, counter);
    }
}

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
}