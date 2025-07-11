using System.Security.Cryptography;
using Toolbox.Extensions;

namespace Toolbox.Tools;

public static class RandomTool
{
    public static string GenerateCode(int digests = 4)
    {
        digests.Assert(x => x >= 4 && x <= 10, "Invalid digests");
        string fmt = $"X{(digests)}";

        string randString = RandomNumberGenerator.GetBytes(digests / 2).Func(x => BitConverter.ToUInt16(x, 0).ToString(fmt));
        return randString;
    }
}
