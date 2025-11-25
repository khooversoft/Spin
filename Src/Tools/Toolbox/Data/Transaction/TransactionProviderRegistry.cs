using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

namespace Toolbox.Data;

public class TransactionProviderRegistry
{
    private readonly ConcurrentHashSet<ITransactionRegister> _providers = new();
    private readonly ILogger<TransactionProviderRegistry> _logger;

    public TransactionProviderRegistry(ILogger<TransactionProviderRegistry> logger) => _logger = logger.NotNull();

    public void Add(ITransactionRegister provider)
    {
        provider.NotNull();
        _providers.TryAdd(provider).Assert(x => x == true, $"Provider already registered");
        _logger.LogInformation("Registered transaction provider providerType");
    }

    public void Remove(ITransactionRegister provider)
    {
        provider.NotNull();
        _providers.TryRemove(provider).Assert(x => x == true, $"Provider not found");
        _logger.LogInformation("Removed transaction provider");
    }

    public IReadOnlyCollection<ITransactionRegister> GetAll() => _providers.ToList().AsReadOnly();
}
