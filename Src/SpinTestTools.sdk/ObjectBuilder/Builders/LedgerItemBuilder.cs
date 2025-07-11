using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using Toolbox.Extensions;
using Toolbox.Logging;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class LedgerItemBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var test = new OptionTest();

        var client = new Lazy<SoftBankClient>(() => service.GetRequiredService<SoftBankClient>());

        foreach (var ledgerItem in option.LedgerItems)
        {
            var addResponse = await client.Value.AddLedgerItem(ledgerItem.AccountId, ledgerItem, context);
            addResponse.LogStatus(context, "Add ledger item accountId={accountId}, amount={amount}", [ledgerItem.AccountId], ledgerItem.Amount.ToString());
            test.Test(() => addResponse);
        }

        return test;
    }

    public Task<Option> Delete(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        // Ledger items cannot be deleted
        return new Option(StatusCode.OK).ToTaskResult();
    }
}
