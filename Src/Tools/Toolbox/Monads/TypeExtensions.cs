using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.Monads;

namespace Toolbox.Monads;

public static class TypeExtensions
{
    public static Maybe<TValue> TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : class
    {
        bool found = dictionary.TryGetValue(key, out var value);
        return found ? new Maybe<TValue>(value) : Maybe<TValue>.None;
    }
}
