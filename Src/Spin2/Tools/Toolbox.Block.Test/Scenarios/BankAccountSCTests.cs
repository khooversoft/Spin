////using System.Collections.Generic;
////using FluentAssertions;
////using Microsoft.Extensions.DependencyInjection;
////using Toolbox.Block.Test.Scenarios.Bank;
////using Toolbox.Block.Test.Scenarios.Bank.Models;
////using Toolbox.DocumentContainer;
////using Toolbox.Extensions;
////using Toolbox.Security.Principal;
////using Toolbox.Types;
////using Toolbox.Types.Maybe;

////namespace Toolbox.Block.Test.Scenarios;

////public class BankAccountSCTests
////{
////    private (string description, LedgerType type, decimal amount)[] _firstList = new (string description, LedgerType type, decimal amount)[]
////    {
////            ("trans 1", LedgerType.Credit, 100.00m),
////            ("trans 2", LedgerType.Credit, 210.25m),
////            ("trans 3", LedgerType.Debit, 50.15m),
////    };

////    private (string description, LedgerType type, decimal amount)[] _secondList = new (string description, LedgerType type, decimal amount)[]
////    {
////            ("trans 4", LedgerType.Debit, 45.00m),
////            ("trans 5", LedgerType.Credit, 30.50m),
////            ("trans 6", LedgerType.Debit, 10.00m),
////            ("trans 7", LedgerType.Debit, 15.00m),
////    };

////    private (string description, LedgerType type, decimal amount)[] _thirdList = new (string description, LedgerType type, decimal amount)[]
////    {
////            ("trans 8", LedgerType.Credit, 1000.00m),
////            ("trans 9", LedgerType.Credit, 500.00m),
////    };

////    private const string _accountName = "accountName1";
////    private const string _owner = "user@domain.com";
////    private const string _path = "default/bank1";
////    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");
////    private readonly IServiceProvider _serviceProvider;
////    private readonly ScopeContext _context = new ScopeContext();

////    public BankAccountSCTests()
////    {
////        _serviceProvider = new ServiceCollection()
////            .AddLogging()
////            .AddSingleton<BankAccountSCActor>()
////            .AddSingleton<IDocumentStore, DocumentStoreInMemory>()
////            .AddSingleton<DocumentLease>()
////            .BuildServiceProvider();
////    }

////    [Fact]
////    public async Task BankAcountCreationTest()
////    {
////        await CreateDocument();
////    }

////    [Fact]
////    public async Task<BankAccountBlock> BankAccounModificationTest()
////    {
////        DateTime now = DateTime.UtcNow;

//        BankAccountSC sc = await CreateDocument();

////        AccountMaster accountMaster = sc.GetAccountMaster();

////        accountMaster = accountMaster with
////        {
////            UpdateDate = now,
////            Counter = accountMaster.Counter + 1,
////            Message = "Update entry",
////        };

////        sc.Add(accountMaster, _owner);
////        sc.Sign();
////        sc.Validate();

////        AccountMaster accountMaster2 = sc.GetAccountMaster();
////        (accountMaster == accountMaster2).Should().BeTrue();

////        sc.GetBalance().Should().Be(GetDataBalance(_firstList));

////        var actor = _serviceProvider.GetRequiredService<BankAccountSCActor>();
////        await actor.Set(sc, _context);

////        return sc;
////    }

//    [Fact]
//    public async Task<BankAccountSC> BankAccounLedgerTest()
//    {
//        DateTime now = DateTime.UtcNow;

//        BankAccountSC sc = await CreateDocument();

////        AccountMaster accountMaster = sc.GetAccountMaster();

////        _secondList.ForEach(x => sc.AddLedger(x.description, x.type, x.amount, _owner));

////        sc.Add(accountMaster, _owner);
////        sc.Sign();
////        sc.Validate();

////        IReadOnlyList<LedgerItem> itemOptions = sc.GetLedgerItems().Return();
////        itemOptions.Count.Should().Be(7);

////        var compareTo = itemOptions
////            .Select(x => (x.Description, x.Type, x.Amount))
////            .ToArray();

////        sc.GetBalance().Should().Be(GetDataBalance(_firstList.Concat(_secondList)));
////        Enumerable.SequenceEqual(_firstList.Concat(_secondList), compareTo).Should().BeTrue();

////        _thirdList.ForEach(x => sc.AddLedger(x.description, x.type, x.amount, _owner));
////        sc.Sign();
////        sc.Validate();
////        sc.GetBalance().Should().Be(GetDataBalance(_firstList.Concat(_secondList).Concat(_thirdList)));

////        IReadOnlyList<LedgerItem> itemOptions2 = sc.GetLedgerItems().Return();
////        itemOptions2.Count.Should().Be(9);

////        return sc;
////    }

//    private async Task<BankAccountSC> CreateDocument()
//    {
//        var actor = _serviceProvider.GetRequiredService<BankAccountSCActor>();

//        BankAccountSC sc = await actor.Create((DocumentId)_path, _accountName, _owner, _context);

////        _firstList.ForEach(x => sc.AddLedger(x.description, x.type, x.amount, _owner));

////        sc.GetBalance().Should().Be(100.00m + 210.25m + -50.15m);

////        Option<AccountMaster> accountMaster = sc.GetAccountMaster();
////        accountMaster.HasValue.Should().BeTrue();
////        accountMaster.Value.AccountName.Should().Be(_accountName);
////        accountMaster.Value.OwnerPrincipleId.Should().Be(_owner);

////        AccountMaster accountMaster2 = sc.GetAccountMaster();
////        accountMaster2.AccountName.Should().Be(_accountName);
////        accountMaster2.OwnerPrincipleId.Should().Be(_owner);

////        Option<IReadOnlyList<LedgerItem>> itemOptions = sc.GetLedgerItems();
////        itemOptions.HasValue.Should().BeTrue();
////        itemOptions.Value.Count.Should().Be(3);

////        var compareTo = itemOptions.Value
////            .Select(x => (x.Description, x.Type, x.Amount))
////            .ToArray();

////        Enumerable.SequenceEqual(_firstList, compareTo).Should().BeTrue();

////        sc.Add(_ownerSignature);
////        sc.Sign();
////        sc.Validate();

////        await actor.Set(sc, _context);

////        return sc;
////    }

////    private decimal GetDataBalance(IEnumerable<(string description, LedgerType type, decimal amount)> values) => values
////        .Select(x => x.type.NaturalAmount(x.amount))
////        .Sum();


////}
