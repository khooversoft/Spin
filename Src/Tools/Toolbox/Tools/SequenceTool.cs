using System.Security.Cryptography;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class SequenceTool
{
    public static string GenerateId()
    {
        string randString = RandomNumberGenerator.GetBytes(2).Func(x => BitConverter.ToUInt16(x, 0).ToString("X4"));

        var result = $"{DateTime.UtcNow:yyyyMMdd-HHmmss-fff}-{randString}";
        return result;
    }
}
