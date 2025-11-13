//using System.Collections.Frozen;
//using System.Collections.Immutable;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Toolbox.Extensions;
//using Toolbox.Store;
//using Toolbox.Tools;
//using Toolbox.Types;

//namespace Toolbox.Data;

//public class TransactionManagerFactory
//{
//    private IServiceProvider? _serviceProvider;

//    public TransactionManagerFactory() { }
//    public TransactionManagerFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider.NotNull();
//    public TransactionManagerFactory(LogSequenceNumber logSequenceNumber, IListStore<DataChangeRecord> changeClient, IServiceProvider serviceProvider)
//    {
//        LogSequenceNumber = logSequenceNumber.NotNull();
//        ChangeClient = changeClient.NotNull();
//        _serviceProvider = serviceProvider.NotNull();
//    }

//    public Sequence<ITransactionProvider> Providers { get; } = new();
//    public LogSequenceNumber? LogSequenceNumber { get; set; }
//    public IListStore<DataChangeRecord>? ChangeClient { get; set; }
//    public string? JournalKey { get; set; }

//    public TransactionManagerFactory SetServiceProvider(IServiceProvider serviceProvider) => this.Action(x => x._serviceProvider = serviceProvider.NotNull());
//    public TransactionManagerFactory SetJournalKey(string journalKey) => this.Action(x => x.JournalKey = journalKey.NotEmpty());
//    public TransactionManagerFactory SetJournalKey(LogSequenceNumber logSequenceNumber) => this.Action(x => x.LogSequenceNumber = logSequenceNumber.NotNull());
//    public TransactionManagerFactory SetJournalKey(IListStore<DataChangeRecord> changeClient) => this.Action(x => x.ChangeClient = changeClient.NotNull());

//    public TransactionManagerFactory AddProvider(ITransactionProvider provider)
//    {
//        provider.NotNull();
//        provider.Name.NotEmpty();
//        Providers.Any(x => x.Name.Equals(provider.Name, StringComparison.OrdinalIgnoreCase)).Assert(x => x == false, "Provider with name={name} already exists", provider.Name);

//        Providers.Add(provider);
//        return this;
//    }

//    public TransactionManager Build()
//    {
//        _serviceProvider.NotNull("Service provider is null");
//        JournalKey.NotEmpty("JournalKey must be set");
//        LogSequenceNumber.NotNull("LogSequenceNumber must be set");
//        ChangeClient.NotNull("ChangeClient must be set");
//        Providers.Count.Assert(x => x > 0, "At least one provider must be added");

//        var config = new TransactionConfiguration(JournalKey, Providers);
//        var manager = ActivatorUtilities.CreateInstance<TransactionManager>(_serviceProvider, config, LogSequenceNumber, ChangeClient);

//        return manager;
//    }
//}

//public sealed class TransactionConfiguration
//{
//    private readonly ImmutableArray<ITransactionProvider> _providersList;

//    public TransactionConfiguration(string journalKey, IEnumerable<ITransactionProvider> providers)
//    {
//        providers.NotNull().Count().Assert(x => x > 0, "At least one provider must be provided");

//        Providers = providers.ToFrozenDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);
//        _providersList = providers.ToImmutableArray();
//        JournalKey = journalKey.NotEmpty();
//    }

//    public FrozenDictionary<string, ITransactionProvider> Providers { get; }
//    public IReadOnlyList<ITransactionProvider> ProviderList => _providersList;
//    public string JournalKey { get; }
//}

