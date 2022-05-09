using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Tools;

namespace Toolbox.Extensions;

public static class DictionaryExtensions
{
    public static bool IsEqual(this IEnumerable<KeyValuePair<string, string>> left, IEnumerable<KeyValuePair<string, string>> right)
    {
        if (left == null || right == null) return false;
        if (left.Count() != right.Count()) return false;

        return left.OrderBy(x => x.Key)
            .Zip(right.OrderBy(x => x.Key), (o, i) => (o, i))
            .All(x => x.o.Key == x.i.Key && x.o.Value == x.i.Value);
    }

    public static TValue Get<TKey, TValue>(
        this IReadOnlyDictionary<TKey, TValue> directory,
        TKey key,
        string? objectType = null,
        string? message = null,
        ILogger? logger = null,
        [CallerMemberName] string function = "",
        [CallerFilePath] string path = "",
        [CallerLineNumber] int lineNumber = 0
        )
    {
        directory.NotNull(nameof(directory), function: function, path: path, lineNumber: lineNumber);

        directory.TryGetValue(key, out TValue? value)
            .Assert(x => x == true, x => message ?? $"{objectType ?? string.Empty}{x} not found", logger: logger, function: function, path: path, lineNumber: lineNumber);

        return value!;
    }
}
