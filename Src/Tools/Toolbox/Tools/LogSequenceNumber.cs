using System.Security.Cryptography;
using Toolbox.Extensions;
using Toolbox.Types;

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
}
