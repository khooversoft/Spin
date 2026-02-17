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

    /// <summary>
    /// Generates a cryptographically secure random hexadecimal string of the specified length.
    /// </summary>
    /// <param name="hexLength">
    /// The length of the hexadecimal string to generate. Must be a positive even number less than 1,048,576 (1MB).
    /// Default value is 8.
    /// </param>
    /// <returns>
    /// A lowercase hexadecimal string containing random characters [0-9a-f] of the specified length.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when hexLength is not positive, not even, or exceeds 1,048,575 characters.
    /// </exception>
    /// <remarks>
    /// This method uses <see cref="RandomNumberGenerator"/> to generate cryptographically secure random bytes,
    /// which are then converted to a lowercase hexadecimal representation. For strings up to 256 characters,
    /// stack allocation is used for performance; larger strings use heap allocation.
    /// 
    /// The generated string is suitable for use as secure tokens, session IDs, or other cryptographic purposes
    /// where unpredictability is important.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Generate an 8-character hex string (default)
    /// string token = RandomTool.GenerateRandomSequence();
    /// // Result: "a1b2c3d4"
    /// 
    /// // Generate a 16-character hex string
    /// string longToken = RandomTool.GenerateRandomSequence(16);
    /// // Result: "f7e8d9c0b1a29384"
    /// </code>
    /// </example>
    public static string GenerateRandomSequence(int hexLength = 8)
    {
        hexLength.Assert(x => x > 0 && (x & 1) == 0 && x < 1024 * 1024, x => $"{x} hexLength must be a positive even number.");

        int byteLen = hexLength / 2;

        // Generate random bytes securely
        byte[] randomBytes = new byte[byteLen];
        RandomNumberGenerator.Fill(randomBytes);

        // Convert bytes -> lowercase hex using Span<char>
        Span<char> hexSpan = hexLength <= 256
            ? stackalloc char[hexLength]  // stack for small sizes
            : new char[hexLength];        // heap for larger sizes

        int pos = 0;
        foreach (byte b in randomBytes)
        {
            hexSpan[pos++] = GetHexChar(b >> 4);   // high nibble
            hexSpan[pos++] = GetHexChar(b & 0xF);  // low nibble
        }

        return new string(hexSpan);


        // Lowercase hex [0-9a-f]
        static char GetHexChar(int nibble) => (char)(nibble < 10 ? ('0' + nibble) : ('a' + (nibble - 10)));
    }
}
