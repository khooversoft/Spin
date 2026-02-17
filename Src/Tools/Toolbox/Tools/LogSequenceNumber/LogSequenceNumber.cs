using System.Security.Cryptography;
using Toolbox.Extensions;

namespace Toolbox.Tools;

/// <summary>
/// Generates monotonically increasing log sequence numbers combining a timestamp (UTC milliseconds),
/// a per-instance counter, and a random 2-byte hex suffix. Also parses LSN strings into <see cref="Lsn"/>.
/// </summary>
public class LogSequenceNumber
{
    private long _counter = 0;

    public long GetCounter() => _counter;

    /// <summary>
    /// Returns the next log sequence number in the format "{timestampMillis:D15}-{counter:D6}-{randomHex}".
    /// </summary>
    public string Next()
    {
        var counter = Interlocked.Increment(ref _counter);

        string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));
        DateTimeOffset now = DateTimeOffset.UtcNow;

        var result = $"{now.ToUnixTimeMilliseconds():D15}-{counter.ToString("D6")}-{randString}";
        return result;
    }

    /// <summary>
    /// Parses a log sequence number string or returns <see cref="Lsn.Default"/> when null/empty.
    /// </summary>
    /// <param name="logSequenceNumber">log sequence number or null</param>
    /// <returns>Lsn</returns>
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
