using System.Collections.Concurrent;
using Toolbox.Tools;

namespace TicketShareWeb.Application;

public class FormValidation
{
    private ConcurrentDictionary<string, string> _errors = new(StringComparer.OrdinalIgnoreCase);

    public string? this[string key]
    {
        get => _errors.TryGetValue(key, out string? errorMessage) ? errorMessage : null;
        set => Add(key, value.NotEmpty());
    }

    public bool HasErrors() => _errors.Count > 0;
    public void Clear() => _errors.Clear();

    public void Add(string key, string errorMessage) => _errors[key.NotEmpty()] = errorMessage.NotEmpty();
    public void Remove(string key) => _errors.TryRemove(key, out _);
    public bool TryGetValue(string key, out string? errorMessage) => _errors.TryGetValue(key, out errorMessage);
}
