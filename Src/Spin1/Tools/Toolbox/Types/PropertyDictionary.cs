using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Types;


public class PropertyDictionary : IEnumerable<KeyValuePair<string, object>>
{
    private readonly ConcurrentDictionary<string, object> _properties = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public int Count => _properties.Count;

    public T? Get<T>() => Get<T>(typeof(T).GetTypeName());

    public T? Get<T>(string name)
    {
        name.NotEmpty();

        if (!_properties.TryGetValue(name, out object? value)) return default!;

        (value is T)
            .Assert(x => x == true, $"{name} property returned type {value.GetType()} but generic type is {typeof(T)}");

        return (T)value;
    }

    public void Set<T>(T value) => _properties[typeof(T).GetTypeName()] = value!;

    public void Set<T>(string name, T value) => _properties[name.NotEmpty()] = value!;

    public void Delete<T>() => _properties.TryRemove(typeof(T).GetTypeName(), out var _);

    public void Delete<T>(string name) => _properties.TryRemove(name.NotEmpty(), out var _);

    public void Clear() => _properties.Clear();

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _properties.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _properties.GetEnumerator();
}