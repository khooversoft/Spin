using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Pattern;

public class PatternCollection
{
    private readonly List<KeyValuePair<string, PatternFactory>> _patterns = new List<KeyValuePair<string, PatternFactory>>();
    private readonly List<KeyValuePair<string, Func<PatternContext, object, object>>> _transform = new List<KeyValuePair<string, Func<PatternContext, object, object>>>();
    private readonly object _lock = new object();

    public void Add(string name, Func<PatternContext, object?> factory)
    {
        lock (_lock)
        {
            name.NotEmpty()
                .Assert(x => _patterns.Any(y => string.Equals(y.Key, name, StringComparison.OrdinalIgnoreCase)) == false, $"Duplicate name {name}");

            _patterns.Add(new KeyValuePair<string, PatternFactory>(name, new PatternFactory { Name = name, Factory = factory }));
        }
    }

    public void Add<T>(string name, Func<PatternContext, T, T> select) where T : class, IPatternResult
    {
        _transform.Add(new KeyValuePair<string, Func<PatternContext, object, object>>(name, (context, x) => select(context, (T)x)));
    }

    public (bool Matched, T? Result) TryMatch<T>(string data) where T : class, IPatternResult => (TryMatch(data, out T? result), result);

    public bool TryMatch<T>(string data, [NotNullWhen(true)] out T? result) where T : class, IPatternResult
    {
        result = default;

        if (data.IsEmpty()) return false;

        var context = new PatternContext()
        {
            Source = data
        };

        (string Key, object? Factory) varResult = _patterns
            .Select(x => (x.Key, Factory: x.Value.Factory(context)))
            .SkipWhile(x => x.Factory == default)
            .Take(1)
            .FirstOrDefault();

        if (varResult == default || varResult.Factory == default) return false;

        result = (T)varResult.Factory;

        result = Transform(context, result) ?? result;
        return true;
    }

    private T? Transform<T>(PatternContext context, T input)
        where T : class, IPatternResult
    {
        var result = _transform
            .Where(x => string.Equals(input.Name, x.Key, StringComparison.OrdinalIgnoreCase))
            .Select(x => x.Value(context, input))
            .SkipWhile(x => x == default)
            .Take(1)
            .FirstOrDefault();

        if (result == null) return null;
        return (T)result;
    }
}
