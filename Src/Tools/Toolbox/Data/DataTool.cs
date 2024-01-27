using Toolbox.Tools;

namespace Toolbox.Data;

public static class DataTool
{
    public static ReadOnlySpan<byte> RemoveBOM(ReadOnlySpan<byte> data)
    {
        if (data.Length > 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF) return data[3..];
        return data;
    }

    public static class Unicode
    {
        private const int _length = 6;

        public static int? CheckCommon(ReadOnlySpan<char> slice)
        {
            if (slice.Length < _length) return null;

            if (slice[0] != '\\') return null;
            if (slice[1] != 'u') return null;
            if (!IsDigetOrUpper(slice[2])) return null;
            if (!IsDigetOrUpper(slice[3])) return null;
            if (!IsDigetOrUpper(slice[4])) return null;
            if (!IsDigetOrUpper(slice[5])) return null;

            return _length;
        }

        public static int? CheckStandard(ReadOnlySpan<char> slice)
        {
            if (slice.Length < _length) return null;

            if (slice[0] != 'U') return null;
            if (slice[1] != '+') return null;
            if (!IsDigetOrUpper(slice[2])) return null;
            if (!IsDigetOrUpper(slice[3])) return null;
            if (!IsDigetOrUpper(slice[4])) return null;
            if (!IsDigetOrUpper(slice[5])) return null;

            return _length;
        }
    }

    public static bool IsDigetOrUpper(char chr)
    {
        if (char.IsDigit(chr)) return true;
        if (char.IsUpper(chr)) return true;
        return false;
    }

    public static T[] Filter<T>(T[] input, Func<T, bool> isValid, Func<T, T>? convert = null)
    {
        isValid.NotNull();
        if (input.Length == 0) return input;

        // Count the number of ASCII characters
        int resultLength = 0;
        ReadOnlySpan<T> inputSpan = input;

        for (int i = 0; i < inputSpan.Length; i++)
        {
            if (isValid(inputSpan[i])) resultLength++;
        }

        if (inputSpan.Length == resultLength) return input;

        // Allocate a Span<char> for reduced
        T[] resultBuffer = new T[resultLength];
        Span<T> resultSpan = resultBuffer;
        convert ??= x => x;

        int index = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (isValid(inputSpan[i])) resultSpan[index++] = convert(inputSpan[i]);
        }

        return resultBuffer;
    }

    public static string Filter(string input, Func<char, bool> isValid, Func<char, char>? convert = null)
    {
        isValid.NotNull();
        if (input.Length == 0) return input;

        // Count the number of ASCII characters
        int resultLength = 0;
        convert ??= x => x;
        ReadOnlySpan<char> inputSpan = input;

        bool willConvert = false;
        for (int i = 0; i < inputSpan.Length; i++)
        {
            if (convert(inputSpan[i]) != inputSpan[i]) willConvert = true;
            if (isValid(inputSpan[i])) resultLength++;
        }

        if (!willConvert && inputSpan.Length == resultLength) return input;

        // Allocate a Span<char> for reduced
        Span<char> resultSpan = new char[resultLength];

        int index = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (isValid(inputSpan[i])) resultSpan[index++] = convert(inputSpan[i]);
        }

        return new string(resultSpan);
    }

    public static bool IsAsciiRange(char chr) => chr >= 32 && chr < 127;
}
