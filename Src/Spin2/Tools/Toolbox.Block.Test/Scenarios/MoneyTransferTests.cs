//using FluentAssertions;
//using Microsoft.Extensions.DependencyInjection;
//using Toolbox.Block.Test.Scenarios.Bank;
//using Toolbox.Block.Test.Scenarios.Bank.Models;
//using Toolbox.DocumentContainer;
//using Toolbox.Security.Principal;
//using Toolbox.Tools;
//using Toolbox.Types;
//using Toolbox.Types.Maybe;

//namespace Toolbox.Block.Test.Scenarios;

///// <summary>
///// Transfer of money from on SC to SC from issuer.  Issuer is one of the SC
///// 
///// (1) Push transfer from sc to sc
///// (2) Pull transfer to sc from sc
///// 
///// Rule:
///// (1) Any SC can issue a push or pull transfer
///// 
///// </summary>
//public class MoneyTransferTests
//{
//    private const string _owner = "user@domain.com";
//    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");

//    [Fact]
//    public async Task PushTransferTests()
//    {
//        const string bank1Path = "default/bank1/Account1";
//        const string bank2Path = "default/bank2/Account2";

//        var services = new ServiceCollection()
//            .AddLogging()
//            //.AddBank()
//            .BuildServiceProvider();

//        var host = services.GetRequiredService<BankHost>();

//        BankSC bank1 = await host.Create(bank1Path, "bank1", _owner).Return();
//        BankSC bank2 = await host.Create(bank2Path, "bank1", _owner).Return();

//        await host.Start(bank1, ScopeContext.Default);
//        await host.Start(bank2, ScopeContext.Default);

//        //BankAccountBlock bank1 = await CreateDocument("bank1", bank1Path, services);
//        //BankAccountBlock bank2 = await CreateDocument("bank2", bank2Path, services);

//        //var broker1 = await services.GetRequiredService<BankSC>().Start(bank1, host, bank1Path, _owner, ScopeContext.Default);
//        //var broker2 = await services.GetRequiredService<BankSC>().Start(bank2, host, bank2Path, _owner, ScopeContext.Default);
//        var message = services.GetRequiredService<IMessageBroker>();

//        var command = new PushTransfer
//        {
//            ToPath = bank2Path,
//            FromPath = bank1Path,
//            Amount = 100.00m,
//        };

//        TransferResult result = await message.Call<PushTransfer, TransferResult>($"{bank1Path}/push", command, ScopeContext.Default);
//        result.Should().NotBeNull();
//        result.Status.Should().Be(StatusCode.OK);

//        var balance1 = bank1.AccountBlock.GetBalance();
//        var balance2 = bank2.AccountBlock.GetBalance();

//        balance1.Should().Be(-100.00m);
//        balance2.Should().Be(100.00m);
//    }

//    //private async Task<BankAccountBlock> CreateDocument(string accountName, string path, IServiceProvider serviceProvider)
//    //{
//    //    var actor = serviceProvider.GetRequiredService<BankAccountSCActor>();
//    //    BankAccountBlock sc = await actor.Create((DocumentId)path, accountName, _owner, ScopeContext.Default);

//    //    sc.Add(_ownerSignature);
//    //    sc.Sign();
//    //    sc.Validate();

//    //    await actor.Set(sc, ScopeContext.Default);

//    //    return sc;
//    //}
//}

