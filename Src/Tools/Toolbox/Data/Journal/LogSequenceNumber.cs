using System.Security.Cryptography;
using Toolbox.Extensions;

namespace Toolbox.Data;

public class LogSequenceNumber
{
    private long _counter = 0;

    public long GetCounter() => _counter;

    public string Next()
    {
        var counter = Interlocked.Increment(ref _counter);

        string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

        var result = $"{DateTime.UtcNow:yyyyMMdd}-{counter.ToString("d10")}-{randString}";
        return result;
    }
}
