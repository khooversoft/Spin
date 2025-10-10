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
            if (!IsDigitOrUpper(slice[2])) return null;
            if (!IsDigitOrUpper(slice[3])) return null;
            if (!IsDigitOrUpper(slice[4])) return null;
            if (!IsDigitOrUpper(slice[5])) return null;

            return _length;
        }

        public static int? CheckStandard(ReadOnlySpan<char> slice)
        {
            if (slice.Length < _length) return null;

            if (slice[0] != 'U') return null;
            if (slice[1] != '+') return null;
            if (!IsDigitOrUpper(slice[2])) return null;
            if (!IsDigitOrUpper(slice[3])) return null;
            if (!IsDigitOrUpper(slice[4])) return null;
            if (!IsDigitOrUpper(slice[5])) return null;

            return _length;
        }
    }

    public static bool IsDigitOrUpper(char chr)
    {
        if (char.IsDigit(chr)) return true;
        if (char.IsUpper(chr)) return true;
        return false;
    }
}
