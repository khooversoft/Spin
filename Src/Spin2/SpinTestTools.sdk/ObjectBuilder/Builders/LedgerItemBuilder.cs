using Microsoft.Extensions.DependencyInjection;
using SoftBank.sdk.SoftBank;
using Toolbox.Extensions;
using Toolbox.Types;

namespace SpinTestTools.sdk.ObjectBuilder.Builders;

public class LedgerItemBuilder : IObjectBuilder
{
    public async Task<Option> Create(IServiceProvider service, ObjectBuilderOption option, ScopeContext context)
    {
        var test = new OptionTest();

        SoftBankClient softBankClient = service.GetRequiredService<SoftBankClient>();

        foreach (var ledgerItem in option.LedgerItems)
        {
            var addResponse = await softBankClient.AddLedgerItem(ledgerItem.AccountId, ledgerItem, context);
            context.Trace().LogStatus(addResponse, "Add ledger item accountId={accountId}, amount={amount}", ledgerItem.AccountId, ledgerItem.Amount);
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
