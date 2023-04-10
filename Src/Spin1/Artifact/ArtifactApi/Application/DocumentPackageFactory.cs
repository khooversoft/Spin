using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Toolbox.Abstractions.Tools;
using Toolbox.Azure.DataLake;
using Toolbox.Azure.DataLake.Model;
using Toolbox.DocumentStore;
using Toolbox.Extensions;

namespace ArtifactApi.Application;

public class DocumentPackageFactory
{
    private readonly Dictionary<string, DatalakeStoreOption> _options;
    private readonly ConcurrentDictionary<string, DocumentPackage> _active = new ConcurrentDictionary<string, DocumentPackage>(StringComparer.OrdinalIgnoreCase);
    private readonly ILoggerFactory _loggerFactory;

    public DocumentPackageFactory(IEnumerable<(string Container, DatalakeStoreOption Option)> options, ILoggerFactory loggerFactory)
    {
        options.NotNull();
        options.Assert(x => x.Count() > 0, $"{nameof(options)} is empty");
        loggerFactory.NotNull();

        _options = options.ToDictionary(x => x.Container, x => x.Option, StringComparer.OrdinalIgnoreCase);
        _loggerFactory = loggerFactory;
    }

    public bool Exist(string container) => _options.ContainsKey(container);

    public DocumentPackage Create(string container)
    {
        container.NotEmpty();

        return _active.GetOrAdd(container, x =>
        {
            _options.TryGetValue(container, out DatalakeStoreOption? option)
                .Assert(x => x == true, $"Cannot find container {container}");

            DatalakeStore store = new DatalakeStore(option!, _loggerFactory.CreateLogger<DatalakeStore>());
            return new DocumentPackage(store, _loggerFactory.CreateLogger<DocumentPackage>());
        });
    }
}
