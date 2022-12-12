using System;
using Toolbox.Tokenizer.Token;

namespace Toolbox.Tokenizer.Syntax
{
    /// <summary>
    /// Block syntax handles block sub data that is marked by a delimiter such as quote ("), single (').
    /// The block start and ending signal must be the same.
    /// Handles escaping characters with "\" (back slash)s
    /// </summary>
    public struct BlockSyntax : ITokenSyntax
    {
        public BlockSyntax(char blockSignal = '"')
        {
            BlockSignal = blockSignal;
            Priority = 1;
        }

        public char BlockSignal { get; }

        public int Priority { get; }

        public int? Match(ReadOnlySpan<char> span)
        {
            if (span.Length == 0) return null;
            if (span[0] != BlockSignal) return null;

            bool isEscape = false;
            for (int index = 1; index < span.Length; index++)
            {
                if (isEscape)
                {
                    isEscape = false;
                    if (span[index] != BlockSignal) throw new ArgumentException("Invalid escape sequence");
                    continue;
                }

                if (span[index] == '\\')
                {
                    isEscape = true;
                    continue;
                }

                if (span[index] == BlockSignal) return index + 1;
            }

            throw new ArgumentException("Missing ending quote");
        }

        public IToken CreateToken(ReadOnlySpan<char> span)
        {
            string value = span.ToString();
            return new BlockToken(value);
        }
    }
}