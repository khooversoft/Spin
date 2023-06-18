using System;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Tokenizer.Syntax;
using Toolbox.Tokenizer.Token;

namespace Toolbox.Tokenizer
{
    /// <summary>
    /// String tokenizer, parses string for values and tokens based on token syntax
    /// </summary>
    public class StringTokenizer
    {
        private readonly List<ITokenSyntax> _syntaxList = new();

        /// <summary>
        /// Return white space tokens that have been collapsed.
        /// </summary>
        /// <returns>this</returns>
        public StringTokenizer UseCollapseWhitespace()
        {
            _syntaxList.Add(new WhiteSpaceSyntax());
            return this;
        }

        /// <summary>
        /// Return single quoted blocks as a token
        /// </summary>
        /// <returns></returns>
        public StringTokenizer UseSingleQuote()
        {
            _syntaxList.Add(new BlockSyntax('\''));
            return this;
        }

        /// <summary>
        /// Return double quoted blocks as a token
        /// </summary>
        /// <returns></returns>
        public StringTokenizer UseDoubleQuote()
        {
            _syntaxList.Add(new BlockSyntax('"'));
            return this;
        }

        /// <summary>
        /// Add tokens to be used in parsing
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public StringTokenizer Add(params string[] tokens)
        {
            _syntaxList.AddRange(tokens.Select(x => (ITokenSyntax)new TokenSyntax(x)));
            return this;
        }

        /// <summary>
        /// Add token syntax used in parsing
        /// </summary>
        /// <param name="tokenSyntaxes"></param>
        /// <returns></returns>
        public StringTokenizer Add(params ITokenSyntax[] tokenSyntaxes)
        {
            _syntaxList.AddRange(tokenSyntaxes);
            return this;
        }

        /// <summary>
        /// Parse strings for tokens
        /// </summary>
        /// <param name="sources">n number of strings</param>
        /// <returns>list of tokens</returns>
        public IReadOnlyList<IToken> Parse(params string[] sources) => Parse(string.Join(string.Empty, sources));

        /// <summary>
        /// Parse string for tokens
        /// </summary>
        /// <param name="source">source</param>
        /// <returns>list of tokens</returns>
        public IReadOnlyList<IToken> Parse(string? source)
        {
            var tokenList = new List<IToken>();

            if (source == null || source == string.Empty) return tokenList;

            ITokenSyntax[] syntaxRules = _syntaxList
                .OrderByDescending(x => x.Priority)
                .ToArray();

            int? dataStart = null;

            ReadOnlySpan<char> span = source.AsSpan();

            for (int index = 0; index < span.Length; index++)
            {
                int? matchLength = null;

                for (int syntaxIndex = 0; syntaxIndex < syntaxRules.Length; syntaxIndex++)
                {
                    matchLength = syntaxRules[syntaxIndex].Match(span.Slice(index));
                    if (matchLength == null)
                    {
                        continue;
                    }

                    if (dataStart != null)
                    {
                        string dataValue = span
                            .Slice((int)dataStart, index - (int)dataStart)
                            .ToString();

                        tokenList.Add(new TokenValue(dataValue));
                        dataStart = null;
                    }

                    tokenList.Add(syntaxRules[syntaxIndex].CreateToken(span.Slice(index, (int)matchLength)));
                    break;
                }

                if (matchLength == null)
                {
                    dataStart ??= index;
                    continue;
                }

                index += (int)matchLength - 1;
            }

            if (dataStart != null)
            {
                string dataValue = span
                    .Slice((int)dataStart, span.Length - (int)dataStart)
                    .ToString();

                tokenList.Add(new TokenValue(dataValue));
            }

            return tokenList;
        }
    }
}