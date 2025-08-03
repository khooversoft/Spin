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

    public static DateTime ConvertToDateTime(string logSequenceNumber)
    {
        logSequenceNumber.NotEmpty();

        long.TryParse(logSequenceNumber[..15], out long milliseconds).Assert(x => x == true, "Invalid log sequence number format");
        return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
    }
}
