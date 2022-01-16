using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Security.Extensions;

public static class ObjectExtensions
{
    /// <summary>
    /// Convert bytes to SHA256 hash
    /// </summary>
    /// <param name="inputBytes">input bytes</param>
    /// <returns>hash as base 64</returns>
    public static string ToSHA256Hash(this IEnumerable<byte> inputBytes)
    {
        inputBytes.VerifyNotNull(nameof(inputBytes));

        return SHA256.Create()
            .ComputeHash(inputBytes.ToArray())
            .Func(Convert.ToBase64String);
    }
}
