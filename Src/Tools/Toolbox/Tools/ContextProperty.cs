using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Toolbox.Tools;

public interface IContextProperty
{
    void Delete<T>();
    void Delete<T>(string name);
    T Get<T>();
    T Get<T>(string name);
    void Set<T>(string name, T value);
    void Set<T>(T value);
}


public class ContextProperty : IContextProperty, IEnumerable<KeyValuePair<string, object>>
{
    private readonly ConcurrentDictionary<string, object> _context = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public T Get<T>() => Get<T>(typeof(T).ToString());

    public T Get<T>(string name)
    {
        name.NotEmpty();

        if (!_context.TryGetValue(name, out object? value)) return default!;

        (value is T).Assert(x => x == true, $"{name} property returned type {value.GetType()} but generic type is {typeof(T)}");
        return (T)value;
    }

    public void Set<T>(T value) => _context[typeof(T).ToString()] = value!;

    public void Set<T>(string name, T value) => _context[name.NotEmpty()] = value!;

    public void Delete<T>() => _context.TryRemove(typeof(T).ToString(), out var _);

    public void Delete<T>(string name) => _context.TryRemove(name.NotEmpty(), out var _);

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _context.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _context.GetEnumerator();
}