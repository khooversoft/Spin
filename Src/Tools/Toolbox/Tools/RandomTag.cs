using System.Security.Cryptography;

namespace Toolbox.Tools;

public static class RandomTag
{
    public static string Generate(int length)
    {
        string key = RandomNumberGenerator.GetHexString(length);
        return key;
    }
}
