using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Security.Principal;
using Toolbox.Tools;
using Toolbox.Types;
using Toolbox.Types.Maybe;

namespace Toolbox.Block.Test.Scenarios;

/// <summary>
/// Transfer of money from on SC to SC from issuer.  Issuer is one of the SC
/// 
/// (1) Push transfer from sc to sc
/// (2) Pull transfer to sc from sc
/// 
/// Rule:
/// (1) Any SC can issue a push or pull transfer
/// 
/// </summary>
public class MoneyTransferTests
{
    private const string _owner = "user@domain.com";
    private readonly PrincipalSignature _ownerSignature = new PrincipalSignature(_owner, _owner, "userBusiness@domain.com");

    [Fact]
    public async Task PushTransferTests()
    {
        const string bank1Path = "default/bank1/Account1";
        const string bank2Path = "default/bank2/Account2";

        var services = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IMessageBroker, MessageBrokerEmulator>()
            .AddSingleton<ITimeContext, TimeContext>()
            .AddTransient<BankBroker>()
            .BuildServiceProvider();

        BankAccountSC bank1 = CreateDocument("bank1");
        BankAccountSC bank2 = CreateDocument("bank2");

        var broker1 = await services.GetRequiredService<BankBroker>().Start(bank1, bank1Path, _owner, new ScopeContext());
        var broker2 = await services.GetRequiredService<BankBroker>().Start(bank2, bank2Path, _owner, new ScopeContext());
        var message = services.GetRequiredService<IMessageBroker>();

        var command = new PushTransfer
        {
            ToPath = bank2Path,
            FromPath = bank1Path,
            Amount = 100.00m,
        };

        TransferResult result = await message.Send<PushTransfer, TransferResult>($"{bank1Path}/push", command, new ScopeContext());
        result.Should().NotBeNull();
        result.Status.Should().Be(OptionStatus.OK);

        var balance1 = bank1.GetBalance();
        var balance2 = bank2.GetBalance();

        balance1.Should().Be(-100.00m);
        balance2.Should().Be(100.00m);
    }

    private BankAccountSC CreateDocument(string accountName)
    {
        BankAccountSC sc = BankAccountSC.Create(accountName, _owner);

        sc.Add(_ownerSignature);
        sc.Sign();
        sc.Validate();

        return sc;
    }
}

