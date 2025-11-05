using System.Collections.Frozen;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Tools;
using Toolbox.Types;

namespace Toolbox.Data;

public class TransactionManagerBuilder
{
    public Sequence<ITransactionProvider> Providers { get; } = new();
    public string? JournalKey { get; set; }
    public IServiceProvider? ServiceProvider { get; set; }

    public TransactionManagerBuilder AddProvider(ITransactionProvider provider)
    {
        provider.NotNull();
        provider.Name.NotEmpty();
        Providers.Any(x => x.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase)).Assert(x => x == false, "Provider with name={name} already exists", provider.Name);

        Providers.Add(provider);
        return this;
    }

    public TransactionManagerBuilder SetJournalKey(string journalKey) => this.Action(x => x.JournalKey = journalKey.NotEmpty());
    public TransactionManagerBuilder SetServiceProvider(IServiceProvider serviceProvider) => this.Action(x => x.ServiceProvider = serviceProvider.NotNull());

    public TransactionManager Build()
    {
        JournalKey.NotEmpty("JournalKey must be set");
        ServiceProvider.NotNull("ServiceProvider must be set");
        Providers.Count.Assert(x => x > 0, "At least one provider must be added");

        var config = new TransactionConfiguration(JournalKey, Providers);
        var manager = ActivatorUtilities.CreateInstance<TransactionManager>(ServiceProvider, config);
        return manager;
    }
}

public sealed class TransactionConfiguration
{
    private readonly ImmutableArray<ITransactionProvider> _providersList;

    public TransactionConfiguration(string journalKey, IEnumerable<ITransactionProvider> providers)
    {
        providers.NotNull().Count().Assert(x => x > 0, "At least one provider must be provided");

        Providers = providers.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
        _providersList = providers.ToImmutableArray();
        JournalKey = journalKey.NotEmpty();
    }

    public FrozenDictionary<string, ITransactionProvider> Providers { get; }
    public IReadOnlyList<ITransactionProvider> ProviderList => _providersList;
    public string JournalKey { get; }
}

