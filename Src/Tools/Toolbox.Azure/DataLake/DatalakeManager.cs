using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Azure;

public interface IDatalakeManagerConfigure
{
    IDatalakeManagerConfigure Add(string storageName, DatalakeOption option);
    IDatalakeManagerConfigure AddMap(string pattern, string storageName, string? prepend = null);
}

public interface IDatalakeManager
{
    Option<string> MapToPath(string path);
    Option<IDatalakeStore> MapToStore(string path);
}

public class DatalakeManager : IDatalakeManager, IDatalakeManagerConfigure
{
    private readonly ConcurrentDictionary<string, DatalakeOption> _register = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, MapToPattern> _map = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, IDatalakeStore> _store = new(StringComparer.OrdinalIgnoreCase);
    private readonly IServiceProvider _serviceProvider;

    public DatalakeManager(IServiceProvider service) => _serviceProvider = service.NotNull();

    public IDatalakeManagerConfigure Add(string storageName, DatalakeOption option)
    {
        storageName.NotEmpty();
        option.Validate().ThrowOnError();

        _register.TryAdd(storageName, option).Assert(x => x == true, $"Storage name={storageName} already exists");
        return this;
    }

    public IDatalakeManagerConfigure AddMap(string pattern, string storageName, string? prepend = null)
    {
        pattern.NotEmpty();
        storageName.NotEmpty();

        var mapToPattern = new MapToPattern(pattern, storageName, prepend);
        _map.TryAdd(pattern, mapToPattern).Assert(x => x == true, $"Pattern={pattern} already exists");
        return this;
    }

    public Option<IDatalakeStore> MapToStore(string path)
    {
        var patternOption = MapPathToStorage(path);
        if (patternOption.IsError()) return patternOption.ToOptionStatus<IDatalakeStore>();

        if (_store.TryGetValue(patternOption.Value.StorageName, out var currentStore)) return currentStore.ToOption();
        if (!_register.TryGetValue(patternOption.Value.StorageName, out var storeOption)) return (StatusCode.NotFound, $"StorageName={patternOption.Value.StorageName} not found");

        IDatalakeStore store = (IDatalakeStore)ActivatorUtilities.CreateInstance(_serviceProvider, typeof(DatalakeStore), storeOption).NotNull();
        _store.TryAdd(patternOption.Value.StorageName, store);
        return store.ToOption();
    }

    public Option<string> MapToPath(string path)
    {
        var patternOption = MapPathToStorage(path);
        if (patternOption.IsError()) return patternOption.ToOptionStatus<string>();

        return patternOption.Value.Prepend switch
        {
            null => path,
            var v => v + (v.EndsWith("/") ? string.Empty : "/") + path,
        };
    }

    private Option<MapToPattern> MapPathToStorage(string path)
    {
        path.NotEmpty(nameof(path));

        foreach (var item in _map)
        {
            if (path.Like(item.Key)) return item.Value;
        }

        return (StatusCode.NotFound, $"Path={path} does not match any pattern");
    }


    private readonly struct MapToPattern
    {
        public MapToPattern(string pattern, string storageName, string? prepend)
        {
            Pattern = pattern.NotNull();
            StorageName = storageName.NotNull();
            Prepend = prepend;
        }

        public string Pattern { get; }
        public string StorageName { get; }
        public string? Prepend { get; }
    }
}
