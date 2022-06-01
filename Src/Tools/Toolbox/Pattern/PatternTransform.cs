using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tokenizer;
using Toolbox.Tokenizer.Token;
using Toolbox.Tools;

namespace Toolbox.Pattern
{
    /// <summary>
    /// Path pattern works like asp.net core router
    /// 
    /// Example...
    /// Pattern: file://{namespace}/{environment}/config/path.ext
    /// Source: file://standard/ppe/config/path.ext
    /// 
    /// Returns:
    ///     namespace = "standard"
    ///     environment = "ppe"
    ///     
    /// </summary>
    public class PatternTransform
    {
        public bool TryMatch(PatternSearch pathPatternSearch, string source, [NotNullWhen(true)] out PatternResult? pathPatternResult)
        {
            pathPatternSearch
                .NotNull()
                .Pattern.NotEmpty();

            pathPatternResult = null;
            if (source.IsEmpty()) return false;

            // Tokenized 
            IReadOnlyList<IToken> patternTokens = new StringTokenizer()
                .Add("{", "}")
                .Parse(pathPatternSearch.Pattern);

            var matches = new List<string>();
            var names = new List<string>();
            var nameValues = new List<string>();

            var cursor = new Cursor<IToken>(patternTokens);

            while (cursor.TryNextValue(out IToken? item))
            {
                if (item.Value == "{")
                {
                    if (!cursor.TryNextValue(out IToken? propertyName)) return false;
                    names.Add(propertyName.Value);

                    if (!cursor.TryNextValue(out propertyName)) return false;
                    if (propertyName.Value != "}") return false;
                    continue;
                }

                matches.Add(item.Value);
            }

            int start = 0;
            var matchCursor = new Cursor<string>(matches);

            while (matchCursor.TryNextValue(out string? matchItem))
            {
                int indexOf = source.IndexOf(matchItem, start);
                if (indexOf < 0) return false;

                if (indexOf > start)
                {
                    nameValues.Add(source.Substring(start, indexOf - start));
                }

                start = indexOf + matchItem.Length;
            }

            if (start < source.Length)
            {
                nameValues.Add(source.Substring(start));
            }

            if (names.Count != nameValues.Count) return false;

            pathPatternResult = new PatternResult
            {
                Name = pathPatternSearch.Name,
                Pattern = pathPatternSearch.Pattern,
                Source = source,
                Values = new ConcurrentDictionary<string, string>(names.Zip(nameValues, (o, i) => new KeyValuePair<string, string>(o, i)), StringComparer.OrdinalIgnoreCase),
            };

            return true;
        }
    }
}
