using System.Security.Cryptography;

namespace Toolbox.TransactionLog;

public class LogSequenceNumber
{
    private long _counter = 0;

    public long GetCounter() => _counter;

    public string Next()
    {
        var counter = Interlocked.Increment(ref _counter);

        byte[] four_bytes = RandomNumberGenerator.GetBytes(2);
        string randString = BitConverter.ToUInt16(four_bytes, 0).ToString("X4");

        return $"{DateTime.UtcNow:yyyyMMdd-HH}-{counter.ToString("d04")}-{randString}";
    }
}
