using System.Security.Cryptography;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class SequenceTool
{
    public static string GenerateId()
    {
        string randString = RandomNumberGenerator.GetBytes(3).Func(x => BitConverter.ToUInt16(x, 0).ToString("X6"));

        var result = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{randString}";
        return result;
    }
}
